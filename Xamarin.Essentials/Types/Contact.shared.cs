using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Essentials
{
    public class Contact
    {
        public Contact()
        {
        }

        public Contact(string name, IEnumerable<ContactPhone> numbers, IEnumerable<ContactEmail> email, ContactType contactType)
        {
            Name = name;
            Emails = email.ToList();
            Numbers = numbers.ToList();
            ContactType = contactType;
        }

        public string Name { get; set; }

        public ContactType ContactType { get; set; }

        public IReadOnlyList<ContactPhone> Numbers { get; set; }

        public IReadOnlyList<ContactEmail> Emails { get; set; }
    }

    public class ContactEmail
    {
        public ContactEmail()
        {
        }

        public ContactEmail(string email, ContactType contactType)
        {
            EmailAddress = email;
            ContactType = contactType;
        }

        public string EmailAddress { get; set; }

        public ContactType ContactType { get; set; }
    }

    public class ContactPhone
    {
        public ContactPhone()
        {
        }

        public ContactPhone(string phoneNumber, ContactType contactType)
        {
            PhoneNumber = phoneNumber;
            ContactType = contactType;
        }

        public string PhoneNumber { get; set; }

        public ContactType ContactType { get; set; }
    }
}
