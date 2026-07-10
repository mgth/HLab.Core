/*
  HLab.Base
  Copyright (c) 2021 Mathieu GRENET.  All right reserved.

  This file is part of HLab.Base.

    HLab.Base is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    HLab.Base is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MouseControl.  If not, see <http://www.gnu.org/licenses/>.

	  mailto:mathieu@mgth.fr
	  http://www.mgth.fr
*/

using System;
using CommunityToolkit.Mvvm.Messaging;
using HLab.Core.Annotations;

namespace HLab.Core;

/// <summary>
/// IMessagesService facade over CommunityToolkit's WeakReferenceMessenger :
/// subscriptions hold the subscriber weakly, so a collected subscriber never
/// leaks nor gets called back.
/// Note : one subscriber instance can register only once per message type,
/// and static lambdas (no target) are rooted by the bus itself.
/// </summary>
public class MessageBus : IMessagesService
{
    readonly WeakReferenceMessenger _messenger = new();

    public void Publish<T>(T payload) where T : class
        => _messenger.Send(payload);

    public void Subscribe<T>(Action<T> action) where T : class
        => _messenger.Register<T>(action.Target ?? this, (_, m) => action(m));

    public void Unsubscribe<T>(Action<T> action) where T : class
        => _messenger.Unregister<T>(action.Target ?? this);
}
