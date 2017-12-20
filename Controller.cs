using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game2048
{    
    public class Controller
    {
        
        private Controller() { }

        private static Controller _contrller;
        
        private List<Tile> _tiles { get; set; } = new List<Tile>();

        private Control _container { get; set; }

        public Control _scoreBoard { get; set; }

        private bool _stepFinished = true;

        private int _score = 0;

        //保存动画动作 以每个tile作为key, 字典中的list需要并发执行, 而list中的action需要顺序执行
        private Dictionary<Tile, List<Action>> _animates = new Dictionary<Tile, List<Action>>();

        public static Controller Create(Control container)
        {
            if (_contrller == null)
            {
                _contrller = new Controller();
            }
            return _contrller;
        }

        public void Restart()
        {
            _tiles.Clear();
            _stepFinished = true;
            _score = 0;
            _scoreBoard.Text = "0";
            Begin();
        }

        public void Draw(Graphics g)
        {
            foreach (var tile in _tiles.ToList())
            {
                tile.Draw(g);
            }
        }

        public void OnKeyUp(Keys keyCode)
        {
            if (!_stepFinished) return;

            if (!new[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right }.Contains(keyCode)) return;
            
            Action<Tile, Enums.Direction> processTile = new Action<Tile, Enums.Direction>((tile, dir) =>
            {
                Tile t = null;
                while (true)
                {
                    #region - MOVE -
                    t = GetAroundTile(dir, tile);
                    if (t == null)
                    {
                        #region - UP -
                        if (dir == Enums.Direction.UP)
                        {
                            if (tile.YIndex > 0)
                            {
                                tile.YIndex -= 1;
                                if (_animates.ContainsKey(tile))
                                {
                                    _animates[tile].Add(() => MoveToDirection(tile, dir));
                                }
                                else
                                    _animates.Add(tile, new List<Action> { () => MoveToDirection(tile, dir) });
                            }
                            else break;
                        }
                        #endregion
                        #region - DOWN -
                        if (dir == Enums.Direction.DOWN)
                        {
                            if (tile.YIndex < 3)
                            {
                                tile.YIndex += 1;
                                if (_animates.ContainsKey(tile))
                                {
                                    _animates[tile].Add(() => MoveToDirection(tile, dir));
                                }
                                else
                                    _animates.Add(tile, new List<Action> { () => MoveToDirection(tile, dir) });
                            }
                            else break;
                        }
                        #endregion
                        #region - LEFT -
                        if (dir == Enums.Direction.LEFT)
                        {
                            if (tile.XIndex > 0)
                            {
                                tile.XIndex -= 1;
                                if (_animates.ContainsKey(tile))
                                {
                                    _animates[tile].Add(() => MoveToDirection(tile, dir));
                                }
                                else
                                    _animates.Add(tile, new List<Action> { () => MoveToDirection(tile, dir) });
                            }
                            else break;
                        }
                        #endregion
                        #region - RIGHT -
                        if (dir == Enums.Direction.RIGHT)
                        {
                            if (tile.XIndex < 3)
                            {
                                tile.XIndex += 1;
                                if (_animates.ContainsKey(tile))
                                {
                                    _animates[tile].Add(() => MoveToDirection(tile, dir));
                                }
                                else
                                    _animates.Add(tile, new List<Action> { () => MoveToDirection(tile, dir) });
                            }
                            else break;
                        }
                        #endregion
                    }
                    else
                    {
                        break;
                    } 
                    #endregion
                }
                if (t != null && t.Number == tile.Number)
                {
                    #region - MERGE -
                    _tiles.Remove(tile);
                    t.Number *= 2;
                    _score += t.Number;
                    _scoreBoard.Text = _score.ToString();

                    if (_animates.ContainsKey(tile))
                    {
                        _animates[tile].Add(() => Merge(t, tile));
                    }
                    else
                        _animates.Add(tile, new List<Action> { () => Merge(t, tile) }); 
                    #endregion
                }
            });
            
            var ts = _tiles.OrderBy(x =>
            {
                if (keyCode == Keys.Up) return x.YIndex;
                if (keyCode == Keys.Down) return 4 - x.YIndex;
                if (keyCode == Keys.Left) return x.XIndex;
                if (keyCode == Keys.Right) return 4 - x.XIndex;
                return 0;
            });
                        
            ts.ToList().ForEach(tile =>
            {
                if (keyCode == Keys.Up)
                {
                    processTile(tile, Enums.Direction.UP);
                }
                if (keyCode == Keys.Down)
                {
                    processTile(tile, Enums.Direction.DOWN);
                }
                if (keyCode == Keys.Left)
                {
                    processTile(tile, Enums.Direction.LEFT);
                }
                if (keyCode == Keys.Right)
                {
                    processTile(tile, Enums.Direction.RIGHT);
                }
            });
            
            _stepFinished = false;
            Task.Run(() =>
            {
                _animates.Keys.AsParallel().ForAll(k =>
                {
                    _animates[k].ForEach(a => a.Invoke());
                });
            })
            .ContinueWith((obj) =>
            {
                if (_animates.Any())
                {
                    //移动的动画完毕之后, 再进行生成的动画
                    _animates.Clear();
                    InitTiles();
                    _animates.Keys.AsParallel().ForAll(k =>
                    {
                        _animates[k].ForEach(a => a.Invoke());
                    });
                }
                _animates.Clear();
                _stepFinished = true;

                if (GameOver())
                {
                    MessageBox.Show(".GAME OVER.");
                }
            });

        }

        public void Begin()
        {
            InitTiles();
        }

        #region - PRIVATE METHODS -
               

        private void InitTiles()
        {
            var ps = InitPos();
            foreach (var p in ps)
            {
                var tile = new Tile(p.X, p.Y, 100);
                _tiles.Add(tile);
                if (ps.Count == 1)//非首次生成
                    _animates.Add(tile, new List<Action> { () => GenTile(tile) });
            }
        }

        private List<Point> InitPos()
        {
            var n = 2;
            if (_tiles.Any()) n = 1;
            Random r = new Random();
            List<Point> ps = new List<Point>();
            for (int i = 0; i < n; i++)
            {
                var x = r.Next(4);
                var y = r.Next(4);
                var p = new Point(x, y);
                if (!ps.Any(m => m.X == x && m.Y == y)
                    && !_tiles.Any(t => t.XIndex == x && t.YIndex == y))
                {
                    ps.Add(p);
                    i++;
                }
                i--;
            }
            return ps;
        }

        private Tile GetAroundTile(Enums.Direction direction, Tile tile)
        {
            var p = new Point(tile.XIndex, tile.YIndex);
            switch (direction)
            {
                case Enums.Direction.UP:
                    p = new Point(tile.XIndex, tile.YIndex - 1);
                    break;
                case Enums.Direction.DOWN:
                    p = new Point(tile.XIndex, tile.YIndex + 1);
                    break;
                case Enums.Direction.LEFT:
                    p = new Point(tile.XIndex - 1, tile.YIndex);
                    break;
                case Enums.Direction.RIGHT:
                    p = new Point(tile.XIndex + 1, tile.YIndex);
                    break;
            }
            var t = _tiles.FirstOrDefault(x => x.XIndex == p.X && x.YIndex == p.Y);
            return t;
        }

        private bool GameOver()
        {
            if (_tiles.Count < 16)
            {
                return false;
            }
            else if (_tiles.Count == 16)
            {
                var over = true;
                foreach (var item in _tiles.ToList())
                {
                    var arounds = new[] {
                        GetAroundTile(Enums.Direction.UP, item),
                        GetAroundTile(Enums.Direction.DOWN, item),
                        GetAroundTile(Enums.Direction.LEFT, item),
                        GetAroundTile(Enums.Direction.RIGHT, item), };

                    if (arounds.Any(x => x != null && x.Number == item.Number))
                    {
                        over = false;
                        break;
                    }
                }
                return over;
            }
            return false;
        }

        #region - ANIMATION -

        private void MoveToDirection(Tile tile, Enums.Direction direction)
        {
            var units = 10;
            for (int i = 0; i < tile.Len / units; i++)
            {
                var p = new Point();
                if (direction == Enums.Direction.UP)
                    p = new Point(tile.Location.X, tile.Location.Y - units);
                if (direction == Enums.Direction.DOWN)
                    p = new Point(tile.Location.X, tile.Location.Y + units);
                if (direction == Enums.Direction.LEFT)
                    p = new Point(tile.Location.X - units, tile.Location.Y);
                if (direction == Enums.Direction.RIGHT)
                    p = new Point(tile.Location.X + units, tile.Location.Y);

                tile.Location = p;
                Thread.Sleep(1);                
            }
        }

        private void Merge(Tile targetTile, Tile movingTile)
        {
            targetTile.Location = new Point(targetTile.Location.X - 10, targetTile.Location.Y - 10);
            targetTile.Size = new Size(targetTile.Len + 20, targetTile.Len + 20);
            for (int i = 1; i <= 10; i++)
            {
                targetTile.Location = new Point(targetTile.Location.X + 1, targetTile.Location.Y + 1);
                targetTile.Size = new Size(targetTile.Size.Width - 2, targetTile.Size.Height - 2);
                Thread.Sleep(1);
            }
        }

        private void GenTile(Tile tile)
        {
            tile.Location = new Point(tile.Location.X + 40, tile.Location.Y + 40);
            tile.Size = new Size(tile.Size.Width - 80, tile.Size.Height - 80);
            for (int i = 1; i <= 20; i++)
            {
                tile.Location = new Point(tile.Location.X - 2, tile.Location.Y - 2);
                tile.Size = new Size(tile.Size.Width + 4, tile.Size.Height + 4);
                Thread.Sleep(1);
            }
        }

        #endregion

        #endregion
    }
}
