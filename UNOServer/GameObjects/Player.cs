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

		/// <summary>
		/// Выполняет ход
		/// </summary>
		/// <param name="drawPile">Основная колода карт</param>
		/// <param name="previousTurn">Предыдущий ход</param>
		public PlayerTurn PlayTurn(CardDeck drawPile, PlayerTurn previousTurn, ServerObject server) {
			this.server = server;
			//string message = server.GetMessageFromPlayer("Ваш ход.\n:" + ShowCards(), this);

			PlayerTurn turn = new PlayerTurn();
			if (previousTurn.Result == TurnResult.Skip
				|| previousTurn.Result == TurnResult.DrawTwo
				|| previousTurn.Result == TurnResult.WildDrawFour) {
				return ProcessAttack(previousTurn.Card, drawPile);

			// если пред. рез. дикая карта или игрок пропустил ход и у него есть чем походить
			} else if ((previousTurn.Result == TurnResult.WildCard
						  || previousTurn.Result == TurnResult.Attacked
						  || previousTurn.Result == TurnResult.ForceDraw)
						  && HasMatch(previousTurn.DeclaredColor)) {
				turn = PlayMatchingCard(previousTurn.DeclaredColor);

			} else if (HasMatch(previousTurn.Card)) {
				// если у нас есть карта, которой мы можем походить и предыдущий резльтат был обычным
				// то обрабатываем как обычный ход
				turn = PlayMatchingCard(previousTurn.Card);
            } else {
				//уведомляем игрока, что о берет карты.
				turn = DrawCard(previousTurn, drawPile);
			}

			DisplayTurn(turn);

			return turn;
		}

		// обрабатываем взятие из колоды карты
		private PlayerTurn DrawCard(PlayerTurn prevTurn, CardDeck cardDeck) {

			var turn = new PlayerTurn();
			// берем одну карту
			var drawnCard = cardDeck.Draw(1);
			// добавляем игроку
			Cards.AddRange(drawnCard);
			server.GetMessageFromPlayer($"У вас нет карт, которые можно разыграть. Вам выдана карта из колоды: {drawnCard[0].DisplayValue}. \nНажмите Enter для продолжения ", this);
			// если можно походить - предлагаем походить
			if(HasMatch(prevTurn.Card)) {
				turn = PlayMatchingCard(prevTurn.Card);
				turn.Result = TurnResult.ForceDrawPlay;
            } else {
				turn.Result = TurnResult.ForceDraw;
				turn.Card = prevTurn.Card;
            }

			return turn;
        }

		private void DisplayTurn(PlayerTurn currentTurn) {
			if (currentTurn.Result == TurnResult.ForceDraw) {
				Console.WriteLine($"{Name} вынужден взять карту. Он пропускает ход");
				server.BroadcastMessage($"{Name} вынужден взять карту. Он пропускает ход", this);
				server.TargetMessage("Вы взяли карту, но она не может быть разыграна. \nВы пропускаете ход", this);
            }

			if(currentTurn.Result == TurnResult.ForceDrawPlay) {
				Console.WriteLine($"{Name} вынужден взять и разграть карту из колоды.");
				server.BroadcastMessage($"{Name} вынужден взять и разыграть карту из колоды. Он пропускает ход", this);
				server.TargetMessage("Вы взяли и разыграли карту из колоды", this);
			}

			if(currentTurn.Result == TurnResult.PlayedCard
			   || currentTurn.Result == TurnResult.Skip
			   || currentTurn.Result == TurnResult.DrawTwo
			   || currentTurn.Result == TurnResult.WildCard
			   || currentTurn.Result == TurnResult.WildDrawFour
			   || currentTurn.Result == TurnResult.Reversed
			   || currentTurn.Result == TurnResult.ForceDrawPlay) {

				Console.WriteLine($"{Name} разыграл карту {currentTurn.Card.DisplayValue}");
				server.BroadcastMessage($"{Name} разыграл карту {currentTurn.Card.DisplayValue}");
				//server.TargetMessage("Вы взяли и разыграли карту из колоды", this);
				if (currentTurn.Card.Color == CardColor.Wild) {
					Console.WriteLine($"{Name} загадал {currentTurn.Card.Color} цвет");
					server.BroadcastMessage($"{Name} загадал {currentTurn.DeclaredColor} цвет");
				}
				if (currentTurn.Card.Value == CardValue.Reverse) {
					Console.WriteLine($"{Name} изменил направление");
					server.BroadcastMessage($"{Name} изменил направление");
				}
			}

			if (Cards.Count == 1) {
				Console.WriteLine($"{Name} кричит UNO");
				server.BroadcastMessage($"{Name} кричит UNO");
			}
		} 

        private PlayerTurn ProcessAttack(Card currentDiscard, CardDeck cardDeck) {
			PlayerTurn turn = new PlayerTurn();
			turn.Result = TurnResult.Attacked;
			turn.Card = currentDiscard;
			turn.DeclaredColor = currentDiscard.Color;

			if (currentDiscard.Value == CardValue.Skip) {
				Console.WriteLine("Игрок " + Name + " пропускает ход");
				server.BroadcastMessage("Игрок " + Name + " пропускает ход", this);
				server.TargetMessage("Вы пропускаете ход", this);
				return turn;

			} else if (currentDiscard.Value == CardValue.DrawTwo) {
				Console.WriteLine("Игрок " + Name + " берет две карты и пропускает ход");
				server.BroadcastMessage("Игрок " + Name + " берет две карты и пропускает ход", this);
				Cards.AddRange(cardDeck.Draw(2));
				server.TargetMessage("Вы берете две карты и пропускаете ход!", this);
				server.TargetMessage("Ваш новый список карт " + ShowCards(), this);

			} else if (currentDiscard.Value == CardValue.DrawFour) {
				Console.WriteLine("Игрок " + Name + " должен взять четыре карты и пропустить ход");
				server.BroadcastMessage("Игрок " + Name + " должен взять четыре карты и пропустить ход", this);
				server.TargetMessage("Вы берете четыре карты и пропускаете ход!", this);
				Cards.AddRange(cardDeck.Draw(4));

			}

			return turn;
		}

		private short RequestCardNumber() {
			string message = server.GetMessageFromPlayer("Ваши карты. Выберите и введите номер\n" + ShowCards(), this);

            Console.WriteLine("Player: " + message);

            short index = -1;
            while (!Int16.TryParse(message, out index) || !(index > 0 && index <= Cards.Count)) {
                message = server.GetMessageFromPlayer(
                    "Выберите верный номер карты", this);
            }

            return (short) (index - 1);
        }

		public string ShowCards() {
			string message = "";
			int index = 1;
			Cards.ForEach(card => {
				message += $"{index++}: {card.DisplayValue}\n";
			});
			return message;
		}

		private short RequestCardColor() {
			short index = -1;
			string message;
			do {
				message = server.GetMessageFromPlayer(
								"Выберите цвет карты:\n1: Красный\n2: Зеленый\n3: Синий\n4: Желтый", this);

				Console.WriteLine($"{Name}: " + message);
			} while (!Int16.TryParse(message, out index) && !Enum.IsDefined(typeof(CardColor), index));

			return (short) (index - 1);
		}

		private PlayerTurn PlayMatchingCard(List<Card> matching) {

			server.BroadcastMessage($"Ход игрока {Name}", this);
			server.TargetMessage($"Ваш ход.", this);

			var turn = new PlayerTurn();
			turn.Result = TurnResult.PlayedCard;
			Card turnCard;
			bool correct = false;
			do {
				// Запрашиваем номер карты у игрока
				turnCard = Cards[RequestCardNumber()];
				// ожидаем, что он выберет карту, которой можно походить
				while(!matching.Contains(turnCard)) {
					Console.WriteLine(Name + ": Вы не можете разыграть эту карту. Выберите другую");
					server.TargetMessage("Вы не можете разыграть эту карту. Выберите другую", this);
					turnCard = Cards[RequestCardNumber()];
				}
				// если это карта возьми четыре и она единственная, которой можно походить - ходим
				if (turnCard.Value == CardValue.DrawFour)
					if (matching.All(x => x.Value == CardValue.DrawFour)) {
						
						// записываем карту в рузльтат
						turn.Card = turnCard;
						// цвет карты запрашиваем у игрока
						turn.DeclaredColor = (CardColor) RequestCardColor();
						// результат - дикая карта
						turn.Result = TurnResult.WildDrawFour;
						// убираем эту карту из списка карт игрока
						Cards.Remove(turnCard);
						// выходим из цикла
						correct = true;
					} else {
						Console.WriteLine(Name + ": Эта карта может быть разыграна только когда у вас нет других карт, которые можно разыграть");
						server.TargetMessage("Эта карта может быть разыграна только когда у вас нет других карт, которые можно разыграть", this);
						continue;
					}

				// если разыгранная карта - дикая
				if(turnCard.Value == CardValue.Wild) {
					// записываем карту в результат
					turn.Card = turnCard;
					// запрашиваем цвет у игрока
					turn.DeclaredColor = (CardColor)RequestCardColor();
					// результат записываем как выбрана дикая карта
					turn.Result = TurnResult.WildCard;
					// убираем эту карту из списка карт игрока
					Cards.Remove(turnCard);
					correct = true;
					// при обычной карте просто записываем все в параметры и работаем далее
				} else {
					turn.Card = turnCard;
					turn.DeclaredColor = turnCard.Color;
					if (turnCard.Value == CardValue.DrawTwo) {
						turn.Result = TurnResult.DrawTwo;
                    } 
					if(turnCard.Value == CardValue.Skip) {
						turn.Result = TurnResult.Skip;
                    }
					Cards.Remove(turnCard);
					correct = true;
				}

			} while(!correct);

			return turn;
		}

		private PlayerTurn PlayMatchingCard(CardColor color) {
			// определяем карты, которыми игрок может походить
			var matching = Cards.Where(x => x.Color == color || x.Color == CardColor.Wild).ToList();
			// запрашиваем у игрока карту и возращаем в виде хода
			return PlayMatchingCard(matching);
        }

		private PlayerTurn PlayMatchingCard(Card card) {
			// определяем карты, которыми игрок может походить
			var matching = Cards.Where(x => x.Color == card.Color || 
										x.Color == CardColor.Wild ||
										x.Value == card.Value
										).ToList();
			// запрашиваем у игрока карту и возращаем в виде хода
			return PlayMatchingCard(matching);
		}

		private bool HasMatch(CardColor color) {
			return Cards.Any(x => x.Color == color || x.Color == CardColor.Wild);
		}

		private bool HasMatch(Card card) {
			return Cards.Any(x => x.Color == card.Color || x.Value == card.Value || x.Color == CardColor.Wild);
		}

	}//end Player

}