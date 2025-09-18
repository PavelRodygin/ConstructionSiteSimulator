using R3;

namespace CodeBase.Services.EventMediator
{
    public class EventMediator
    {
        //In the context of R3, a Subject<T> serves as a bridge between observables and observers, functioning as both
        //an IObservable<T> and an IObserver<T>. This dual role allows it to emit values to its subscribers
        //(IObservable behavior) and to accept values (IObserver behavior).
        private readonly Subject<PopupOpenedEvent> _popupOpenedSubject = new();
        
        /// <summary>
        /// Publishes an event of type <typeparamref name="T"/> to the appropriate event stream.
        /// If the event is a <see cref="PopupOpenedEvent"/>, it is emitted to subscribers of the popup opened event stream.
        /// </summary>
        /// <typeparam name="T">The type of the event to publish. Must be a class.</typeparam>
        /// <param name="eventArgs">The event object to publish. If null, the method does nothing.</param>
        public void Publish<T>(T eventArgs) where T : class
        {
            if (eventArgs is PopupOpenedEvent popupOpenedEvent) 
                _popupOpenedSubject.OnNext(popupOpenedEvent); 
            // calls _popupOpenedSubject.OnNext(popupOpenedEvent) to notify all subscribers about the event.
        }

        // Converts _popupOpenedSubject to an Observable<PopupOpenedEvent> using .AsObservable().
        // Allows external components to subscribe to popup-opened events.
        //     How It Works:
        // Subscribers can call OnPopupOpenedAsObservable and use operators like .Subscribe to listen for popup events.
        public Observable<PopupOpenedEvent> OnPopupOpenedAsObservable() => 
            _popupOpenedSubject.AsObservable();

        public void Complete() => _popupOpenedSubject.OnCompleted();
    }
}