namespace Lfg.Bot.Modals;

public class UpdateGameModal : IModal
{
    public string Title => "Изменить игру";

    [ModalTextInput("game_name", TextInputStyle.Short, "Название игры", 3, 32)]
    public string Name { get; set; }

}