using System;

struct FieldInfo
{
    int _player;
    public int Player
    {
        get { return _player; }
        set { _player = value; }
    }

    public void ChangeMaxWeight(int weight)
    {
        _weight = Math.Max(_weight, weight);
    }

    public void ChangeMinWeight(int weight)
    {
        _weight = Math.Min(_weight, weight);
    }

    int _weight;
    public int Weight
    {
        get { return _weight; }
        set { _weight = value; }
    }

    int _peakDistance;
    public int PeakDistance
    {
        get { return _peakDistance; }
        set { _peakDistance = value; }
    }

    int _xValue;
    public int X
    {
        get { return _xValue; }
        set { _xValue = value; }
    }

    int _yValue;
    public int Y
    {
        get { return _yValue; }
        set { _yValue = value; }
    }

    public override string ToString()
    {
        return string.Format("[{0},{1}] Player {2}; Ground: {3}; Weight: {4}", _xValue, _yValue, _player, _peakDistance, _weight);
    }
}
