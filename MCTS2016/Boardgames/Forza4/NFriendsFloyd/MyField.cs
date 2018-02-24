using System;

    public class MyField
    {
        FieldInfo[,] _fields;
        int _width;
        int _height;
        int _me;

        public MyField(int[,] fields, int me)
        {
            _me = me;
            _width = fields.GetLength(0);
            _height = fields.GetLength(1);
            _fields = new FieldInfo[_width, _height];

            InitializeFields(fields);

            WeightFree();
            WeightCenter();
            WeightVertical();
            WeightHorizontal();
            WeightDiagonal();
        }

        private void WeightFree()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_fields[x, y].Player == 0)
                        _fields[x, y].ChangeMaxWeight(1);
                }
            }
        }

        private void WeightCenter()
        {
            if (_width <= 4) return;

            for (int x = 3; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_fields[x, y].Player == 0)
                        _fields[x, y].ChangeMaxWeight(2);
                }
            }
        }

        private void InitializeFields(int[,] fields)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _fields[x, y].X = x;
                    _fields[x, y].Y = y;
                    _fields[x, y].Player = fields[x, y];
                    _fields[x, y].Weight = 0;
                    _fields[x, y].PeakDistance = y;

                    if (fields[x, y] != 0)
                    {
                        for (int i = 1; i < _height - y; i++)
                        {
                            if (fields[x, y + i] == 0)
                            {
                                _fields[x, y].PeakDistance = -i;
                                break;
                            }
                        }
                        continue;
                    }

                    for (int i = 0; i < y; i++)
                    {
                        if (fields[x, y - i - 1] != 0)
                        {
                            _fields[x, y].PeakDistance = i;
                            break;
                        }
                    }
                }
            }
        }

        internal FieldInfo[,] FieldInfos
        {
            get { return _fields; }
        }

        private void WeightVertical()
        {
            ConnectFinder cf = new ConnectFinder(_height, _me);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                    cf.Add(_fields[x, y]);

                _fields = cf.AddWeight(_fields);
            }
        }

        private void WeightHorizontal()
        {
            ConnectFinder cf = new ConnectFinder(_width, _me);

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                    cf.Add(_fields[x, y]);

                _fields = cf.AddWeight(_fields);
            }
        }

        private void WeightDiagonal()
        {
            ConnectFinder cfUp = new ConnectFinder(Math.Max(_width, _height), _me);
            ConnectFinder cfDown = new ConnectFinder(Math.Max(_width, _height), _me);

            // iterate through the rows
            for (int y = 0; y < _height; y++)
            {
                // shifting through positions
                for (int z = 0; z < _width; z++)
                {
                    if (y + z < _height)
                        cfUp.Add(_fields[z, y + z]); // we are 'z' units right and up from home
                    if (y - z >= 0)
                        cfDown.Add(_fields[z, y - z]); // we are 'z' units right and down from home
                }

                _fields = cfUp.AddWeight(_fields);
                _fields = cfDown.AddWeight(_fields);
            }

            // iterate through the columns (the first [0,0], we had already)
            for (int x = 1; x < _width; x++)
            {
                // shifting through positions
                for (int z = 0; z < _height; z++)
                {
                    if (x + z < _width)
                        cfUp.Add(_fields[x + z, z]); // we are 'z' units right and up from home
                    if (x - z >= 0)
                        cfDown.Add(_fields[x - z, z]); // we are 'z' units right and down from home
                }

                _fields = cfUp.AddWeight(_fields);
                _fields = cfDown.AddWeight(_fields);
            }
        }
    }
