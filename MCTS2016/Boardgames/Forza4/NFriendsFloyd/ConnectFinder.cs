using System.Collections.Generic;

class ConnectFinder
{
    List<FieldInfo> _fili;
    int _me;
    int _opponent;

    public ConnectFinder(int maxLen, int me)
    {
        _fili = new List<FieldInfo>(maxLen);

        _me = me;
        if (me == 1)
            _opponent = 2;
        else
            _opponent = 1;
    }

    public void Add(FieldInfo fi)
    {
        fi.Weight = 0;
        _fili.Add(fi);
    }

    internal FieldInfo[,] AddWeight(FieldInfo[,] fields)
    {
        CalculateWeight();

        foreach (FieldInfo fi in _fili)
            fields[fi.X, fi.Y].Weight += fi.Weight;

        _fili.Clear();

        return fields;
    }

    private void CalculateWeight()
    {
        if (_fili.Count < 4) return;

        for (int i = 0; i <= _fili.Count - 4; i++)
        {
            WeightOpponent(i);
            WeightMyself(i);
        }
    }

    private void WeightMyself(int index)
    {
        if (_fili[index].Player == 0 &&
            _fili[index + 1].Player == _me &&
            _fili[index + 2].Player == _me &&
            _fili[index + 3].Player == _me)
        {
            FieldInfo fi = _fili[index];
            if (fi.PeakDistance == 0)
                fi.ChangeMaxWeight(5000);
            else
                fi.ChangeMaxWeight(500);
            _fili[index] = fi;
        }
        else if (_fili[index].Player == _me &&
            _fili[index + 1].Player == 0 &&
            _fili[index + 2].Player == _me &&
            _fili[index + 3].Player == _me)
        {
            FieldInfo fi = _fili[index + 1];
            if (fi.PeakDistance == 0)
                fi.ChangeMaxWeight(5000);
            else
                fi.ChangeMaxWeight(500);
            _fili[index + 1] = fi;
        }
        else if (_fili[index].Player == _me &&
            _fili[index + 1].Player == _me &&
            _fili[index + 2].Player == 0 &&
            _fili[index + 3].Player == _me)
        {
            FieldInfo fi = _fili[index + 2];
            if (fi.PeakDistance == 0)
                fi.ChangeMaxWeight(5000);
            else
                fi.ChangeMaxWeight(500);
            _fili[index + 2] = fi;
        }
        else if (_fili[index].Player == _me &&
            _fili[index + 1].Player == _me &&
            _fili[index + 2].Player == _me &&
            _fili[index + 3].Player == 0)
        {
            FieldInfo fi = _fili[index + 3];
            if (fi.PeakDistance == 0)
                fi.ChangeMaxWeight(5000);
            else
                fi.ChangeMaxWeight(500);
            _fili[index + 3] = fi;
        }
    }

    private void WeightOpponent(int index)
    {
        if (_fili[index].Player == 0 &&
            _fili[index + 1].Player == _opponent &&
            _fili[index + 2].Player == _opponent &&
            _fili[index + 3].Player == _opponent)
        {
            FieldInfo fi = _fili[index];
            if (fi.PeakDistance == 0)
                fi.ChangeMaxWeight(1000);
            else
                fi.ChangeMaxWeight(100);
            _fili[index] = fi;
        }
        else if (_fili[index].Player == _opponent &&
            _fili[index + 1].Player == 0 &&
            _fili[index + 2].Player == _opponent &&
            _fili[index + 3].Player == _opponent)
        {
            FieldInfo fi = _fili[index + 1];
            if (fi.PeakDistance == 0)
                fi.ChangeMaxWeight(1000);
            else
                fi.ChangeMaxWeight(100);
            _fili[index + 1] = fi;
        }
        else if (_fili[index].Player == _opponent &&
            _fili[index + 1].Player == _opponent &&
            _fili[index + 2].Player == 0 &&
            _fili[index + 3].Player == _opponent)
        {
            FieldInfo fi = _fili[index + 2];
            if (fi.PeakDistance == 0)
                fi.ChangeMaxWeight(1000);
            else
                fi.ChangeMaxWeight(100);
            _fili[index + 2] = fi;
        }
        else if (_fili[index].Player == _opponent &&
            _fili[index + 1].Player == _opponent &&
            _fili[index + 2].Player == _opponent &&
            _fili[index + 3].Player == 0)
        {
            FieldInfo fi = _fili[index + 3];
            if (fi.PeakDistance == 0)
                fi.ChangeMaxWeight(1000);
            else
                fi.ChangeMaxWeight(100);
            _fili[index + 3] = fi;
        }
    }
}
