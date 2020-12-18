///////////////////////////////////////////////////////////
//  Game.cs
//  Implementation of the Class Game
//  Generated by Enterprise Architect
//  Created on:      16-���-2020 9:58:51
//  Original author: adm-sabitovka
///////////////////////////////////////////////////////////


using System.Collections.Generic;
using System.Linq;
using System.Text;
using UNOServer.ServerObjects;

namespace UNOServer.GameObjects {

	/// <summary>
	/// ���������� ����
	/// </summary>
	public class Game {

		ServerObject server;

		/// <summary>
		/// ������ ����
		/// </summary>
		public CardDeck DeckCards { get; set;}

		/// <summary>
		/// ������ �������
		/// </summary>
		public List<Player> Players { get; set; }

		/// <summary>
		/// ������ ������
		/// </summary>
		public List<Card> ThrowCards { get;  set; }

		/// 
		/// <param name="numberOfPlayers"></param>
		public Game(List<Player> players, ServerObject server) {
			Players = players;
			this.server = server;
			DeckCards = new CardDeck();
			ThrowCards = new List<Card>();

			DeckCards.Shuffle();

            for (int i = 0; i < players.Count; i++) {
				Players[i].Position = i;
            }

			int maxCards = 7 * Players.Count;
			int dealtCards = 0;

			while (dealtCards < maxCards) {
				Players.ForEach(pl => {
					pl.Cards.Add(DeckCards.Cards.First());
					DeckCards.Cards.RemoveAt(0);
					dealtCards++;
				});
            }

			ThrowCards.Add(DeckCards.Cards.First());
			DeckCards.Cards.RemoveAt(0);

			while (ThrowCards.First().Value == CardValue.Wild || ThrowCards.First().Value == CardValue.DrawFour) {
				ThrowCards.Insert(0, DeckCards.Cards.First());
				DeckCards.Cards.RemoveAt(0);
			}

			//Players.ForEach(player => server.TargetMessage(PrepareCardsToSend(player), player));
		}

		private string PrepareCardsToSend(Player player) {
			var stringCards = new StringBuilder();
			stringCards.Append("cards^");
			stringCards.Append(
				string.Join(";", player.Cards.Select(card => $"{(int) card.Value},{(int) card.Color}"))
			);
			return stringCards.ToString();
        }

		/// 
		/// <param name="turn"></param>
		private void AddToDiscardPile(PlayerTurn turn) {

		}

		public void PlayGame() {
            System.Console.WriteLine("������ � ���������");
			int i = 0;
			bool isAscending = true;

			PlayerTurn currentTurn = new PlayerTurn() {
				Result = TurnResult.GameStart,
				Card = ThrowCards.First(),
				DeclaredColor = ThrowCards.First().Color
			};

			server.BroadcastMessage($"������ ����� {currentTurn.Card.DisplayValue}.");

			var currentPlayer = Players[i];
			currentPlayer.PlayTurn(DeckCards, currentTurn, server);
		}

	}//end Game

}