namespace Netmarble.Core
{
	public interface IUIComponent
	{
		void Validate();
		void ValidateAsync(int frameSkip);
	}
}