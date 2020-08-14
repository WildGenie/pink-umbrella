namespace PinkUmbrella.ViewModels
{
    public class GenericRootViewModel<T>: BaseViewModel
    {
        public T Model { get; set; }
    }
}