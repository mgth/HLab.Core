/*
  HLab.Mvvm
  Copyright (c) 2021 Mathieu GRENET.  All right reserved.

  This file is part of HLab.Mvvm.

    HLab.Mvvm is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    HLab.Mvvm is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MouseControl.  If not, see <http://www.gnu.org/licenses/>.

	  mailto:mathieu@mgth.fr
	  http://www.mgth.fr
*/

using System;
using HLab.Mvvm.Annotations;

namespace HLab.Mvvm;

public class ViewModelCache(IMvvmContext context, IMvvmService mvvm)
{
    public object? GetLinked(object? baseObject, Type viewMode, Type viewClass)
    {
        //We cannot retrieve a view for a null object
        if (baseObject == null) return null;

        var context1 = context;

        if (baseObject is IViewModel vm)
        {
            // if the viewModel was created outside, it may not contain a context
            if (vm.MvvmContext == null)
            {
                if (vm is IMvvmContextProvider p)
                {
                    context1 = context1.GetChildContext(p.GetType().Name);
                    p.ConfigureMvvmContext(context1);
                }
                vm.MvvmContext = context1;
            }
            else
                // set current context to be the view model one
                context1 = vm.MvvmContext;
        }

        var linkedType = mvvm.GetLinkedType(baseObject.GetType(), viewMode, viewClass);

        if (linkedType == null) return null;

        return context1.Locate(linkedType, baseObject);
    }

    public IView? GetView(object? baseObject, Type? viewMode, Type? viewClass)
    {
        //TODO : find a solution to identify baseObject type when it's null and retrieve an empty view
        if (baseObject == null) return null;

        viewClass ??= typeof(IDefaultViewClass);
        viewMode ??= typeof(DefaultViewMode);

        while (true)
        {
            var linked = GetLinked(baseObject, viewMode, viewClass);

            switch (linked)
            {
                case null:
                    return mvvm.GetNotFoundView(baseObject.GetType(), viewMode, viewClass);

                case IView fe:
                    mvvm.PrepareView(fe);
                    return fe;

                default:
                    baseObject = linked;
                    break;
            }
        }
    }
}
