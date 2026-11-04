using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace HospitalVR.Tests
{
    public class RcpClockTests
    {
        object clock;
        Type type;
        [SetUp] public void SetUp()
        {
            type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("HospitalVR.RCP.RcpBeatClock")).FirstOrDefault(t => t != null);
            Assert.That(type, Is.Not.Null, "Falta el reloj compartido");
            clock = Activator.CreateInstance(type);
        }
        object Call(string name, params object[] args) => type.GetMethod(name).Invoke(clock, args);
        bool Tick(double now) => (bool)Call("Tick", now);
        double Next => (double)type.GetProperty("NextBeat").GetValue(clock);

        [Test] public void DoesNotDispatchBeforeScheduledInstant()
        {
            Call("Start", 10.0, 110.0);
            Assert.That(Next, Is.EqualTo(10.05).Within(.00001));
            Assert.That(Tick(10.049), Is.False);
            Assert.That(Tick(10.05), Is.True);
        }
        [Test] public void SameFrameCannotDispatchTwice()
        {
            Call("Start", 0.0, 110.0);
            Assert.That(Tick(.05), Is.True);
            Assert.That(Tick(.05), Is.False);
            Assert.That(Next, Is.EqualTo(.595454545).Within(.000001));
        }
        [Test] public void FrameDropSkipsExpiredBeatsWithoutBurst()
        {
            Call("Start", 0.0, 110.0);
            Assert.That(Tick(2.0), Is.False, "No debe emitir una ráfaga atrasada");
            Assert.That(Next, Is.EqualTo(2.23181818).Within(.000001));
            Assert.That(Tick(2.232), Is.True);
            Assert.That(Tick(2.232), Is.False);
        }
        [Test] public void StopClearsClockAndPreventsLaterBeats()
        {
            Call("Start", 0.0, 110.0); Call("Stop");
            Assert.That(Tick(20.0), Is.False);
            Assert.That(Next, Is.EqualTo(0.0));
        }
        [Test] public void RestartDoesNotCatchUpOldSession()
        {
            Call("Start", 0.0, 110.0); Tick(.05); Call("Stop");
            Call("Start", 20.0, 110.0);
            Assert.That(Tick(20.0), Is.False);
            Assert.That(Tick(20.05), Is.True);
        }
    }
}
