﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Rhino.Mocks.Interfaces;
using Is=NUnit.Framework.Is;

namespace Docu.Tests
{
    public static class Extensions
    {
        public static void ShouldBeTrue(this bool actual)
        {
            Assert.That(actual, Is.True);
        }

        public static void ShouldBeFalse(this bool actual)
        {
            Assert.That(actual, Is.False);
        }

        public static object ShouldBeOfType(this object actual, Type expected)
        {
            Assert.AreEqual(expected, actual.GetType());
            return actual;
        }

        public static T ShouldBeOfType<T>(this object actual)
        {
            actual.ShouldNotBeNull();
            actual.ShouldBeOfType(typeof(T));
            return (T)actual;
        }

        public static void ShouldNotBeOfType<T>(this object actual)
        {
            Assert.That(actual, Is.Not.TypeOf<T>());
        }

        public static IEnumerable<T> ShouldContain<T>(this IEnumerable<T> actual, Func<T, bool> expected)
        {
            actual.First(expected).ShouldNotEqual(default(T));
            return actual;
        }

        public static MethodInfo ShouldEqual<T>(this MethodInfo method, Expression<Action<T>> expected)
        {
            var expectedMethod = ((MethodCallExpression)expected.Body).Method;

            Assert.That(method, Is.EqualTo(expectedMethod));
            return method;
        }

        public static PropertyInfo ShouldEqual<T>(this PropertyInfo property, Expression<Func<T, object>> expected)
        {
            var expectedProperty = (PropertyInfo)((MemberExpression)expected.Body).Member;

            Assert.That(property, Is.EqualTo(expectedProperty));
            return property;
        }

        public static T ShouldEqual<T>(this T actual, T expected)
        {
            Assert.That(actual, Is.EqualTo(expected));
            return actual;
        }

        public static T ShouldBeSameAs<T>(this T actual, T expected)
        {
            Assert.That(actual, Is.SameAs(expected));
            return actual;
        }

        public static T ShouldNotEqual<T>(this T actual, T expected)
        {
            Assert.That(actual, Is.Not.EqualTo(expected));
            return actual;
        }

        public static T ShouldBeNull<T>(this T actual)
        {
            Assert.That(actual, Is.Null);
            return actual;
        }

        public static T ShouldNotBeNull<T>(this T actual)
        {
            Assert.That(actual, Is.Not.Null);
            return actual;
        }

        public static XmlNode ToNode(this string original)
        {
            var doc = new XmlDocument();

            doc.LoadXml(original);

            return doc.DocumentElement;
        }

        public static T First<T>(this IEnumerable<T> list)
        {
            return list.FirstOrDefault();
        }

        public static T Second<T>(this IEnumerable<T> list)
        {
            return list.ElementAtOrDefault(1);
        }

        public static CapturingConstraint CaptureArgumentsFor<MOCK>(this MOCK mock, Expression<Action<MOCK>> methodExpression) 
            where MOCK : class
        {
            return CaptureArgumentsFor(mock, methodExpression, o => { });
        }

        public static CapturingConstraint CaptureArgumentsFor<MOCK>(this MOCK mock, Expression<Action<MOCK>> methodExpression, Action<IMethodOptions<RhinoMocksExtensions.VoidType>> optionsAction)
            where MOCK : class
        {
            return CaptureArgumentsFor(mock,
                methodExpression,
                m => m.Expect(methodExpression.Compile()),
                optionsAction);
        }

        public static CapturingConstraint CaptureArgumentsFor<MOCK>(this MOCK mock, Expression<Function<MOCK, object>> methodExpression)
            where MOCK : class
        {
            return CaptureArgumentsFor(mock, methodExpression, o => { });
        }

        public static CapturingConstraint CaptureArgumentsFor<MOCK, RESULT>(this MOCK mock, Expression<Function<MOCK, RESULT>> methodExpression, Action<IMethodOptions<RESULT>> optionsAction)
            where MOCK : class
        {
            return CaptureArgumentsFor(mock,
                methodExpression,
                m => m.Expect(methodExpression.Compile()),
                optionsAction);
        }

        public static CapturingConstraint CaptureArgumentsFor<MOCK, DELEGATETYPE, OPTIONTYPE>(MOCK mock,
                                                            Expression<DELEGATETYPE> methodExpression,
                                                            Func<MOCK, IMethodOptions<OPTIONTYPE>> expectAction,
                                                            Action<IMethodOptions<OPTIONTYPE>> optionsAction)
            where MOCK : class
        {
            var method = ReflectionHelper.GetMethod(methodExpression);
            var constraint = new CapturingConstraint();
            var constraints = new List<AbstractConstraint>();

            method.GetParameters().ForEach(p => constraints.Add(constraint));

            var expectation = expectAction(mock).Constraints(constraints.ToArray()).Repeat.Any();
            optionsAction(expectation);

            return constraint;
        }

        public class CapturingConstraint : AbstractConstraint
        {
            private readonly ArrayList argList = new ArrayList();

            public override string Message
            {
                get { return ""; }
            }

            public override bool Eval(object obj)
            {
                argList.Add(obj);
                return true;
            }

            public T First<T>()
            {
                return ArgumentAt<T>(0);
            }

            public T ArgumentAt<T>(int pos)
            {
                return (T)argList[pos];
            }

            public T Second<T>()
            {
                return ArgumentAt<T>(1);
            }
        }
    }
}
