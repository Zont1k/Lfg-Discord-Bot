namespace Lfg.Bot.Modals;

public class CreateGameModal : IModal
{
    public string Title => "Добавить игру";

    [ModalTextInput("game_name", TextInputStyle.Short, "Название игры", 3, 32)]
    public string Name { get; set; }
}