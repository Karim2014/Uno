///////////////////////////////////////////////////////////
//  Player.cs
//  Implementation of the Class Player
//  Generated by Enterprise Architect
//  Created on:      16-дек-2020 9:59:03
//  Original author: adm-sabitovka
///////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using UNOServer.ServerObjects;

namespace UNOServer.GameObjects {

	/// <summary>
	/// Игрок
	/// </summary>
	public class Player {

		public List<Card> Cards { get; set; }
		public string Name { get;  set; }
		public int Position { get; set; }
		public string Id { get; set; }

		private ServerObject server;

		/// 
		/// <param name="name"></param>
		public Player(string name) {
			Cards = new List<Card>();
			Name = name;
		}

		public string ShowCards() {
			return $"Ваши карты: {string.Join(" ", Cards.Select(с => с.DisplayValue))}";
		}

		/// 
		/// <param name="drawPile"></param>
		/// <param name="currentTurn"></param>
		public PlayerTurn PlayTurn(CardDeck drawPile, PlayerTurn previousTurn, ServerObject server) {
			this.server = server;
			string message = server.GetMessageFromPlayer("Ваш ход.\n:" + ShowCards(), this);

			short index = -1;
			Console.WriteLine("Player: " + message);
			while(!Int16.TryParse(message, out index)) {
				message = server.GetMessageFromPlayer(
					"Выберите верный номер карты", this);
			}

			PlayerTurn turn = new PlayerTurn();
			if (previousTurn.Result == TurnResult.Skip
				|| previousTurn.Result == TurnResult.DrawTwo
				|| previousTurn.Result == TurnResult.WildDrawFour) {
				return ProcessAttack(previousTurn.Card, drawPile);

			} else if ((previousTurn.Result == TurnResult.WildCard
						  || previousTurn.Result == TurnResult.Attacked
						  || previousTurn.Result == TurnResult.ForceDraw)
						  && HasMatch(previousTurn.DeclaredColor)) {
				turn = PlayMatchingCard(previousTurn.DeclaredColor);
			}

			return turn;
			;
		}

        private PlayerTurn ProcessAttack(Card currentDiscard, CardDeck cardDeck) {
			PlayerTurn turn = new PlayerTurn();
			turn.Result = TurnResult.Attacked;
			turn.Card = currentDiscard;
			turn.DeclaredColor = currentDiscard.Color;

			if (currentDiscard.Value == CardValue.Skip) {
				Console.WriteLine("Игрок " + Name + " пропускает ход");
				server.BroadcastMessage("Игрок " + Name + " пропускает ход", Id);
				server.TargetMessage("Вы пропускаете ход", this);
				return turn;

			} else if (currentDiscard.Value == CardValue.DrawTwo) {
				Console.WriteLine("Игрок " + Name + " берет две карты и пропускает ход");
				server.BroadcastMessage("Игрок " + Name + " берет две карты и пропускает ход", Id);
				server.TargetMessage("Вы берете две карты и пропускаете ход!", this);
				Cards.AddRange(cardDeck.Draw(2));

			} else if (currentDiscard.Value == CardValue.DrawFour) {
				Console.WriteLine("Игрок " + Name + " должен взять четыре карты и пропустить ход");
				server.BroadcastMessage("Игрок " + Name + " должен взять четыре карты и пропустить ход");
				server.TargetMessage("Вы берете четыре карты и пропускаете ход!", this);
				Cards.AddRange(cardDeck.Draw(4));

			}

			return turn;
		}

		private short RequestCardNumber() {
			string message = server.GetMessageFromPlayer("Выберите карту:\n" + ShowCards(), this);

			short index = -1;
			Console.WriteLine("Player: " + message);
			while (!Int16.TryParse(message, out index) && index < Cards.Count) {
				message = server.GetMessageFromPlayer(
					"Выберите верный номер карты", this);
			}

			return index;
        }

		private short RequestCardColor() {
			short index = -1;
			string message;
			do {
				message = server.GetMessageFromPlayer(
								"Выберите цвет карты:\n1: Красный\n2: Зеленый\n3: Синий\n4: Желтый", this);

				Console.WriteLine($"{Name}: " + message);
			} while (!Int16.TryParse(message, out index) && !Enum.IsDefined(typeof(CardColor), index));

			return index;
		}

		private PlayerTurn PlayMatchingCard(CardColor color) {
			var turn = new PlayerTurn();
			turn.Result = TurnResult.PlayedCard;
			var matching = Cards.Where(x => x.Color == color || x.Color == CardColor.Wild).ToList();

			//если остались только дикие карты
			if (matching.All(x => x.Value == CardValue.DrawFour)) {
				// играем первой попавшеся
				// TODO: Сдлеать уведомление игрока о том, что у него остались только дикие карты
				turn.Card = matching.First();
				// запрашиваем у игрока цвет карты, который он хочет
				turn.DeclaredColor = (CardColor) RequestCardColor();
				// резльтуат стаонвиться дикая карта на следующего игрока
				turn.Result = TurnResult.WildCard;
				// убираем эту карту из списка карт игрока
				Cards.Remove(matching.First());

				return turn;
			}

			// Запрашиваем номер карты у игрока
			Card turnCard = Cards[RequestCardNumber()];
			// ожидаем, что он выберет карту, которой можно походить
			while (!matching.Contains(turnCard)) {
				turnCard = Cards[RequestCardNumber()];
            }

			turn.Card = turnCard;
			turn.DeclaredColor = turnCard.Color;

			return turn;
        }

		private bool HasMatch(CardColor color) {
			return Cards.Any(x => x.Color == color || x.Color == CardColor.Wild);
		}

		private bool HasMatch(Card card) {
			return Cards.Any(x => x.Color == card.Color || x.Value == card.Value || x.Color == CardColor.Wild);
		}

	}//end Player

}