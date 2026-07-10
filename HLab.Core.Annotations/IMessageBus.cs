using System;

namespace HLab.Core.Annotations;

public interface IMessagesService
{
    void Publish<T>(T payload) where T : class;
    void Subscribe<T>(Action<T> action) where T : class;
    void Unsubscribe<T>(Action<T> action) where T : class;
}
