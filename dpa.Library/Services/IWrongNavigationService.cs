using dpa.Library.Models;

namespace dpa.Library.Services;

public interface IWrongNavigationService
{
    void NavigateTo(Exercise parameter);
    void NavigateBack();
}

public static class WrongNavigationConstant
{
    public const string QuestionView = nameof(QuestionView);
}