using Common.Constant;
using Common.Enums;
using Common.Helper;
using Common.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

namespace DataServiceLib
{
    public class SyncCustomerService
    {
        public static ResponseModel InsertCustomer(CustomerRaw customerRaw, MongoDbHelper<Customer> _mongoDbCustomer)
        {
            var response = new ResponseModel() { IsSuccessful = true };
            try
            {
                var customer = new Customer()
                {
                    OrgId = customerRaw.Id,
                    Birthday = customerRaw.Birthday,
                    CreatedDate = DateTime.Now,
                    OrgCreatedDate = customerRaw.CreatedAt.Value,
                    ChannelSubscribes = new List<int>(),
                    WebsiteId = customerRaw.WebsiteId,
                    TotalOrders = customerRaw.TotalOrders,
                    TotalSpent = customerRaw.TotalSpent,
                    TotalPaid = customerRaw.TotalPaid,
                    TotalPayTimes = customerRaw.TotalPayTimes,
                    AverageOrderValue = customerRaw.AverageOrderValue,
                    TotalProducts = customerRaw.TotalProducts,
                    TotalUniqueProducts = customerRaw.TotalUniqueProducts,
                    BestSelling = new List<Product>(customerRaw.BestSelling),
                    TotalCancelledOrders = customerRaw.TotalCancelledOrders,
                    CompletePaymentTimes = customerRaw.CompletePaymentTimes,
                    TotalDiscountCodeUsed = customerRaw.TotalDiscountCodeUsed
                };

                if (customerRaw.EmailConsent != null && customerRaw.EmailConsent.State == EcommerceFields.EMAIL_SUBSCRIBED) 
                {
                    customer.ChannelSubscribes.Add((int)EChannelSubscribe.Email);
                }
                
                if (customerRaw.AcceptsMarketing != null && customerRaw.AcceptsMarketing == true)
                {
                    customer.ChannelSubscribes.Add((int)EChannelSubscribe.Email);
                }

                if (customerRaw.RFM != null)
                {
                    customer.RFM = new RFM
                    {
                        RValue = customerRaw.RFM.RValue,
                        FValue = customerRaw.RFM.FValue,
                        MValue = customerRaw.RFM.MValue
                    };
                }

                if (customerRaw.Tags != null)
                {
                    var tags = customerRaw.Tags.Split(',');
                    if (!string.IsNullOrEmpty(tags[0]))
                    {
                        customer.Tags.AddRange(tags);
                    }
                }

                if (customerRaw.Addresses != null)
                {
                    foreach (Address address in customerRaw.Addresses)
                    {
                        var contact = new Contact()
                        {
                            FullName = (address.FirstName + " " + address.LastName).Trim(),
                            Phone = address.Phone,
                            Address = address.Address1,
                            OrgId = address.Id,
                            OrgSrc = customerRaw.OrgSrc
                        };
                        if (Regex.Replace(contact.FullName, @"s", "") == "")
                        {
                            contact.FullName = null;
                        }
                        customer.Contacts.Add(contact);
                    }
                }

                var generalContact = new Contact()
                {
                    OrgSrc = customerRaw.OrgSrc,
                    Email = customerRaw.Email,
                    Phone = customerRaw.Phone,
                    FullName = (customerRaw.FirstName + " " + customerRaw.LastName).Trim()
                };
                if (Regex.Replace(generalContact.FullName, @"s", "") == "")
                {
                    generalContact.FullName = null;
                }
                customer.Contacts.Add(generalContact);

                _mongoDbCustomer.Insert(customer);
            }
            catch (Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public static ResponseModel UpdateCustomer(Customer matchedCustomer, CustomerRaw customer, MongoDbHelper<Customer> _mongoDbCustomer)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                matchedCustomer.TotalOrders += customer.TotalOrders;
                matchedCustomer.TotalSpent += customer.TotalSpent;
                matchedCustomer.TotalPaid += customer.TotalPaid;
                matchedCustomer.TotalPayTimes += customer.TotalPayTimes;
                matchedCustomer.TotalProducts += customer.TotalProducts;
                matchedCustomer.TotalUniqueProducts += customer.TotalUniqueProducts;
                matchedCustomer.AverageOrderValue = 
                    matchedCustomer.TotalOrders > 0 ? Math.Round((matchedCustomer.TotalSpent / matchedCustomer.TotalOrders), 2) : 0;
                matchedCustomer.TotalCancelledOrders += customer.TotalCancelledOrders;
                matchedCustomer.CompletePaymentTimes += customer.CompletePaymentTimes;
                matchedCustomer.TotalDiscountCodeUsed += customer.TotalDiscountCodeUsed;
                matchedCustomer.BestSelling.AddRange(customer.BestSelling);
                matchedCustomer.BestSelling = matchedCustomer.BestSelling.OrderByDescending(x => x.Quantity).Take(3).ToList();

                matchedCustomer.ChannelSubscribes = new List<int>();
                if (customer.EmailConsent != null && customer.EmailConsent.State == EcommerceFields.EMAIL_SUBSCRIBED)
                {
                    matchedCustomer.ChannelSubscribes.Add((int)EChannelSubscribe.Email);
                }
                if (customer.AcceptsMarketing != null && customer.AcceptsMarketing == true)
                {
                    matchedCustomer.ChannelSubscribes.Add((int)EChannelSubscribe.Email);
                }

                if (customer.RFM != null)
                {
                    if (matchedCustomer.RFM == null)
                    {
                        matchedCustomer.RFM = new RFM
                        {
                            MValue = customer.RFM.MValue,
                            FValue = customer.RFM.FValue,
                            RValue = customer.RFM.RValue
                        };
                    }
                    else
                    {
                        matchedCustomer.RFM.MValue += customer.RFM.MValue;
                        matchedCustomer.RFM.FValue += customer.RFM.FValue;
                        matchedCustomer.RFM.RValue = Math.Min(matchedCustomer.RFM.RValue, customer.RFM.RValue);
                    }
                }

                if (customer.Addresses != null)
                {
                    foreach (Address address in customer.Addresses)
                    {
                        var contactName = (address.FirstName + " " + address.LastName).Trim();
                        if (Regex.Replace(contactName, @"s", "") == "")
                        {
                            contactName = null;
                        }
                        var existContact = matchedCustomer.Contacts.FirstOrDefault(x =>
                            x.FullName == contactName
                            && x.Phone == address.Phone
                            && x.Address == address.Address1);

                        if (existContact == null)
                        {
                            var contact = new Contact
                            {
                                FullName = contactName,
                                Phone = address.Phone,
                                Address = address.Address1,
                                OrgId = address.Id,
                                OrgSrc = customer.OrgSrc
                            };
                            matchedCustomer.Contacts.Add(contact);
                        }
                    }
                }

                var existGeneralContact = matchedCustomer.Contacts.FirstOrDefault(x =>
                        !string.IsNullOrEmpty(x.Email)
                        && x.Phone == customer.Phone
                        && x.Email == customer.Email);
                if (existGeneralContact == null)
                {
                    var generalContact = new Contact()
                    {
                        OrgSrc = customer.OrgSrc,
                        Email = customer.Email,
                        Phone = customer.Phone,
                        FullName = (customer.FirstName + " " + customer.LastName).Trim()
                    };
                    if (Regex.Replace(generalContact.FullName, @"s", "") == "")
                    {
                        generalContact.FullName = null;
                    }
                    matchedCustomer.Contacts.Add(generalContact);
                }

                _mongoDbCustomer.Update(matchedCustomer.Id, matchedCustomer);
            }
            catch (Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
