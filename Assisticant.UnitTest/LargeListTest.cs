using Assisticant.Collections;
using Assisticant.Fields;
using Assisticant.UnitTest.ContactListData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.UnitTest
{
    [TestClass]
    public class LargeListTest
    {
        [TestMethod]
        public void CanHaveManyPrecedents()
        {
            var contacts = new ObservableList<Contact>(
                Enumerable.Range(0, 10000)
                    .Select(i => new Contact()
                    {
                        FirstName = "FirstName" + i,
                        LastName = "LastName" + i
                    }));

            var sorted = new ComputedList<Contact>(() =>
                from c in contacts
                orderby c.FirstName, c.LastName
                select c);
            Assert.AreEqual("FirstName100", sorted.ElementAt(3).FirstName);

            sorted.ElementAt(3).FirstName = "George";
            Assert.AreEqual("FirstName1000", sorted.ElementAt(3).FirstName);
        }

        class Projection
        {
            private readonly Observable<string> _prefix;
            private readonly Contact _contact;
            private Computed<string> _name;
            
            public Projection(Observable<string> prefix, Contact contact)
            {
                _prefix = prefix;
                _contact = contact;

                _name = new Computed<string>(() =>
                    prefix.Value + _contact.FirstName + _contact.LastName);
            }

            public string Name
            {
                get { return _name; }
            }
        }

        [TestMethod]
        public void CanHaveManyComputeds()
        {
            var prefix = new Observable<string>("Before");

            var contacts = new ObservableList<Contact>(
                Enumerable.Range(0, 10000)
                    .Select(i => new Contact()
                    {
                        FirstName = "FirstName" + i,
                        LastName = "LastName" + i
                    }));

            var projections = new ComputedList<Projection>(() =>
                from c in contacts
                select new Projection(prefix, c));
            string dummy;
            foreach (var projection in projections)
                dummy = projection.Name;
            Assert.AreEqual("BeforeFirstName3LastName3", projections.ElementAt(3).Name);

            prefix.Value = "After";
            foreach (var projection in projections)
                dummy = projection.Name;
            Assert.AreEqual("AfterFirstName3LastName3", projections.ElementAt(3).Name);
        }
    }
}
