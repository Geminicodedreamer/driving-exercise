using System;
using dpa.Library.Models;
using dpa.Library.ViewModels;
using System.Collections.Generic;
using dpa.Library.Services;

namespace dpa.Services;

public class WrongNavigationService : IWrongNavigationService
{

    public void NavigateTo(Exercise parameter)
    {
        ServiceLocator.Current.MainViewModel.SetWrongContent(parameter);
    }

    public void NavigateBack()
    {
        // ServiceLocator.Current.MainViewModel.ReturnBack();
    }
}