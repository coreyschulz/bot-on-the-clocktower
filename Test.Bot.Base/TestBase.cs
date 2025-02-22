﻿using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Test.Bot.Base
{
    public class TestBase
    {
        private readonly Mock m_mockServiceProvider = new();
        protected IServiceProvider GetServiceProvider() => m_mockServiceProvider;

        protected T RegisterService<T>(T service) where T : class
        {
            m_mockServiceProvider.RegisterService(service);
            return service;
        }

        protected Mock<T> RegisterMock<T>() where T : class => RegisterMock(new Mock<T>());

        protected Mock<T> RegisterMock<T>(Mock<T> mock) where T : class
        {
            m_mockServiceProvider.RegisterService(mock.Object);
            return mock;
        }

  
        protected static Exception CreateException(Type exceptionType)
        {
            var exceptionConstructor = exceptionType.GetConstructor(Array.Empty<Type>());
            Assert.NotNull(exceptionConstructor);
            return (Exception)(exceptionConstructor!.Invoke(Array.Empty<object?>()));
        }

        protected static void AssertCompletedTask(Func<Task> runTask) => TestTaskHelper.AssertCompletedTask(runTask);
        protected static T AssertCompletedTask<T>(Func<Task<T>> runTask) => TestTaskHelper.AssertCompletedTask(runTask);
    }
}
