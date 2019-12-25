using Konsole.Tests.WindowTests.Mocks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Konsole.Tests.WindowTests
{
    public class IsTests
    {
        [Test]
        public void IsRealRoot_MockConsole_is_not_a_real_root()
        {
            var c = new MockConsole(20, 20);
            Assert.False(c._isRealRoot);
        }

        [Test]
        public void IsMockConsole_MockConsole_is_not_a_real_root()
        {
            var c = new MockConsole(20, 20);
            Assert.True(c._isMockConsole);
        }

        // this test fails in release mode from within visual studio, but passes in release on command line.

        [Test]
        public void IsRealRoot_window_without_a_parent_is_a_real_Root()
        {
            var c = new Window(new Settings() { HostSizer = new MockSizer() });
            Assert.True(c._isRealRoot);
        }

        [Test]
        public void IsChildWindow_is_true_only_if_there_is_a_parent()
        {
            var parent = new MockConsole(10, 10);
            var child = new Window(parent);
            Assert.True(child._isChildWindow);
            Assert.False(parent._isChildWindow);
        }


    }
}
