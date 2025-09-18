namespace CodeBase.Services.EventMediator
{
    //TODO Абстракцию для событий надо, чтобы грамотно разделять Core и Implementation
    public class PopupOpenedEvent
    {
        //Represents an event that encapsulates the name of the popup that was opened.
        public string PopupName { get; }

        public PopupOpenedEvent(string popupName) => PopupName = popupName;
    }
}