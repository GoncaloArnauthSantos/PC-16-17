using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Serie1Pc;
using System.Threading;

namespace Serie1PcTest
{
    [TestClass]
    public class ThrottledRegionTests
    {
        public ThrottledRegion region;
        public Queue<Exception> exceptionQueue;
        private readonly int ID1 = 1;
        private readonly int ID2 = 2;

        [TestInitialize]
        public void SetUp()
        {
            region = new ThrottledRegion(2, 2, 3600);
            exceptionQueue = new Queue<Exception>();
        }

        public void EnterRegionSuccessfully()
        {
            try
            {
                if (!region.TryEnter(ID1))
                {
                    Assert.Fail();
                }
            }
            catch (ThreadInterruptedException e)
            {
                exceptionQueue.Enqueue(e);
            }
        }

        public void CannotEnterRegion()
        {
            try
            {
                if (region.TryEnter(ID1))
                {
                    Assert.Fail();
                }
            }
            catch (ThreadInterruptedException e)
            {
                exceptionQueue.Enqueue(e);
            }
        }

        [TestMethod]
        public void SimpleThrottledRegionTest()
        {
            Thread t1 = new Thread(EnterRegionSuccessfully);
            Thread t2 = new Thread(EnterRegionSuccessfully);
            Thread t3 = new Thread(EnterRegionSuccessfully);

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            region.Leave(ID1);
            t3.Start();
            t3.Join();

            Assert.AreEqual(0, exceptionQueue.Count);
        }
    }
    
}
