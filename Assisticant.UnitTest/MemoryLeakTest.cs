﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using Assisticant.Fields;

namespace Assisticant.UnitTest
{
    [TestClass]
    public class MemoryLeakTest
    {
#if SILVERLIGHT
        // In Silverlight, Computed is 12 bytes bigger. Why?
        private const int ComputedPlatformOffset = 12;
#else
        private const int ComputedPlatformOffset = 0;
#endif
#if SILVERLIGHT
        // In Silverlight, Observable is 8 bytes smaller. Why?
        private static long ObservablePlatformOffset = -8;
#else
        private static long ObservablePlatformOffset = 0;
#endif

        [TestMethod]
        //[Ignore]
        public void ObservableIsAsSmallAsPossible()
        {
            GC.Collect();
            long start = GC.GetTotalMemory(true);
            Observable<int> newObservable = new Observable<int>();
            newObservable.Value = 42;
            long end = GC.GetTotalMemory(true);

            // Started at 92.
            // Making Precedent a base class: 80.
            // Removing Gain/LoseComputed events: 72.
            // Custom linked list implementation for dependents: 48.
            // Other optimizations: 40.
            // Removed WeakReferenceToSelf: 20.
            Assert.AreEqual(20 + ObservablePlatformOffset, end - start);

            int value = newObservable;
            Assert.AreEqual(42, value);
        }

        [TestMethod]
        //[Ignore]
        public void ComputedIsAsSmallAsPossible()
        {
            GC.Collect();
            long start = GC.GetTotalMemory(true);
            Computed<int> newComputed = new Computed<int>(() => 42);
            long end = GC.GetTotalMemory(true);

            // Started at 260.
            // Making Precedent a base class: 248.
            // Removing Gain/LoseComputed events: 232.
            // Making IsUpToDate no longer a precident: 192.
            // Custom linked list implementation for dependents: 152.
            // Custom linked list implementation for precedents: 112.
            // Other optimizations: 104.
			// Added WeakReferenceToSelf: 108.
            // Removed WeakReferenceToSelf: 104.
            Assert.AreEqual(104 + ComputedPlatformOffset, end - start);

            int value = newComputed;
            Assert.AreEqual(42, value);
        }

        [TestMethod]
        //[Ignore]
        public void SingleDependencyBeforeUpdateIsAsSmallAsPossible()
        {
            GC.Collect();
            long start = GC.GetTotalMemory(true);
            Observable<int> newObservable = new Observable<int>();
            Computed<int> newComputed = new Computed<int>(() => newObservable);
            newObservable.Value = 42;
            long end = GC.GetTotalMemory(true);

            // Started at 336.
            // Making Precedent a base class: 312.
            // Removing Gain/LoseComputed events: 288.
            // Making IsUpToDate no longer a precident: 248.
            // Custom linked list implementation for dependents: 200.
            // Custom linked list implementation for precedents: 160.
            // Other optimizations: 144.
			// Added WeakReferenceToSelf: 148.
            // Removed WeakReferenceToSelf: 124.
			Assert.AreEqual(124 + ObservablePlatformOffset, end - start);

            int value = newComputed;
            Assert.AreEqual(42, value);
        }

        [TestMethod]
        [Ignore]
        public void SingleDependencyAfterUpdateIsAsSmallAsPossible()
        {
            GC.Collect();
            long start = GC.GetTotalMemory(true);
            Observable<int> newObservable = new Observable<int>();
            Computed<int> newComputed = new Computed<int>(() => newObservable);
            newObservable.Value = 42;
            int value = newComputed;
            long end = GC.GetTotalMemory(true);

            // Started at 460.
            // Making Precedent a base class: 436.
            // Removing Gain/LoseComputed events: 412.
            // Making IsUpToDate no longer a precident: 372.
            // Custom linked list implementation for dependents: 308.
            // Custom linked list implementation for precedents: 192.
            // Weak reference to dependents: 208.
            // Other optimizations: 192.
			// Added WeakReferenceToSelf: 196.
            // Removed WeakReferenceToSelf: 168 - 324.
			Assert.AreEqual(324 + ObservablePlatformOffset, end - start);

            value = newComputed;
            Assert.AreEqual(42, value);
        }

        [TestMethod]
        public void DirectComputedObjectCanBeGarbageCollected()
        {
            GC.Collect();
            SourceData observable = new SourceData();
            DirectComputed computed = new DirectComputed(observable);
            observable.SourceProperty = 42;
            Assert.AreEqual(42, computed.ComputedProperty);
            WeakReference weakComputed = new WeakReference(computed);

            GC.Collect();
            Assert.IsTrue(weakComputed.IsAlive, "Since we hold a strong reference to the dependent, the object should still be alive.");
            // This assertion here to make sure the dependent is not optimized away.
            Assert.AreEqual(42, computed.ComputedProperty);

            computed = null;
            GC.Collect();
            Assert.IsFalse(weakComputed.IsAlive, "Since we released the strong reference to the dependent, the object should not be alive.");

            // Make sure we can still modify the observable.
            observable.SourceProperty = 32;
            Assert.AreEqual(32, observable.SourceProperty);
        }

        [TestMethod]
        public void IndirectComputedObjectCanBeGarbageCollected()
        {
            GC.Collect();
            SourceData observable = new SourceData();
            DirectComputed intermediate = new DirectComputed(observable);
            IndirectComputed indirectComputed = new IndirectComputed(intermediate);
            observable.SourceProperty = 42;
            Assert.AreEqual(42, indirectComputed.ComputedProperty);
            WeakReference weakIndirectComputed = new WeakReference(indirectComputed);

            GC.Collect();
            Assert.IsTrue(weakIndirectComputed.IsAlive, "Since we hold a strong reference to the dependent, the object should still be alive.");
            // This assertion here to make sure the dependent is not optimized away.
            Assert.AreEqual(42, indirectComputed.ComputedProperty);

            indirectComputed = null;
            GC.Collect();
            Assert.IsFalse(weakIndirectComputed.IsAlive, "Since we released the strong reference to the dependent, the object should not be alive.");

            // Make sure we can still modify the observable, and that the intermediate still depends upon it.
            observable.SourceProperty = 32;
            Assert.AreEqual(32, observable.SourceProperty);
            Assert.AreEqual(32, intermediate.ComputedProperty);
        }
    }
}
