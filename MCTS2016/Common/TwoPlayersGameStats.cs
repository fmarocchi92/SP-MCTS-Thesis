#define TRUESKILL

using System;

#if TRUESKILL
using Moserware.Skills;
#endif

using System.Collections.Generic;

namespace Common
{
	public class TwoPlayersGameStats
	{
		public TwoPlayersGameStats ()
		{
			int[] _wins = new int[2]{0,0};
			int[,] _split_wins = new int[2, 2]{ { 0, 0 }, { 0, 0 } };

			int _ties = 0;

			// true skill
			InitTrueSkill();
		}

		public void Reset ()
		{
			int[] _wins = new int[2]{0,0};
			int[,] _split_wins = new int[2, 2]{ { 0, 0 }, { 0, 0 } };

			int _ties = 0;

			// true skill
			InitTrueSkill();
		}

		public void PlayersTied() { _ties++; }

		public void PlayerWon(int p, int sp = 0)
		{
			if (p == 1)
				UpdateTrueSkill (1, 2);
			else 
				UpdateTrueSkill (2, 1);
			
			_wins [p - 1]++;

			if (sp != 0) {
				_split_wins [p - 1, sp - 1]++;
			}
		}

		public int Ties() { return _ties; }

		public int Wins(int player, int starting_player = 0) { 
			
			if (starting_player == 0)
				return _wins [player - 1];
			else return _split_wins[player-1, starting_player-1]; 
		}

		public string PrettyPrintRates() {
			float	sum = ((float)(_wins [0] + _wins [1]) + _ties);

			if (sum==0f)
				return string.Format ("{0:F2}\t{1:F2}\t{2:F2}", 0, 0, 0);
			
			float	wins1p = ((float)_wins [0]) / sum;
			float	wins2p = ((float)_wins [1]) / sum;
			float	tiesp = ((float)_ties) / sum;

			string printed = string.Format ("{0:F2}\t{1:F2}\t{2:F2}", wins1p, wins2p, tiesp);

			printed += "\t" + PlayerRating(1).ToString() + "\t" + PlayerRating(2).ToString() + "\t" + string.Format("{0:F3}",_match_quality);

			return printed;
		}

		#region PRIVATE
		private int[] _wins = new int[2]{0,0};
		private int[,] _split_wins = new int[2,2]{{0,0},{0,0}};
		private int _ties = 0;
		#endregion

		private void InitTrueSkill() {
			_gameInfo = GameInfo.DefaultGameInfo;

			_player1 = new Player (1);
			_rating1 = _gameInfo.DefaultRating;

			_player2 = new Player (2);
			_rating2 = _gameInfo.DefaultRating;
		}

		Rating PlayerRating(int p) {
			if (_rating == null)
				return _gameInfo.DefaultRating;

			if (p == 1)
				return _rating [_player1];
			else
				return _rating [_player2];
		}

		private void UpdateTrueSkill(int player1_position, int player2_position) {

			// setup teams with updated ratings
			var _team1 = new Team(_player1, _rating1);
			var _team2 = new Team(_player2, _rating2);
			var _teams = Teams.Concat(_team1, _team2);

			// update the ratings
			_rating = TrueSkillCalculator.CalculateNewRatings(_gameInfo, _teams, player1_position, player2_position);
			_rating1 = _rating [_player1];
			_rating2 = _rating [_player2];
			_match_quality = TrueSkillCalculator.CalculateMatchQuality (_gameInfo, _teams);
		}

	    public double Quality
	    {
	        get { return _match_quality; }
	    }

#if TRUESKILL
		private IDictionary<Player,Rating> _rating = null; 
		private Player 		_player1;
		private Rating		_rating1;

		private Player 		_player2;
		private Rating		_rating2;

		private GameInfo	_gameInfo = GameInfo.DefaultGameInfo;

		private double		_match_quality = 0f;
		#endif
	}
}

