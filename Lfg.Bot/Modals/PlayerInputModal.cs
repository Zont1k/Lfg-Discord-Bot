namespace Lfg.Bot.Modals
{
    public class PlayerInputModal : IModal
    {
        public string Title => "Установка искомых игроков";

        [RequiredInput(true)]
        [InputLabel("Введите количество искомых игроков.")]
        [ModalTextInput("player_count", placeholder: "1-99")]
        public string PlayerCount { get; set; }
    }
}