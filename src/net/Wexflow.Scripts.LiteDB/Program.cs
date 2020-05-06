using System;
using System.Configuration;
using Wexflow.Core.Db.LiteDB;
using Wexflow.Scripts.Core;

namespace Wexflow.Scripts.LiteDB
{
    class Program
    {
        static void Main()
        {
            try
            {
                var db = new Db(ConfigurationManager.AppSettings["connectionString"]);
                Helper.InsertWorkflowsAndUser(db);

                //for (int i = 0; i < 100; i++)
                //{
                //    var notification = new Notification
                //    {
                //        AssignedBy = "1",
                //        AssignedOn = DateTime.Now.AddDays(-i),
                //        AssignedTo = "1",
                //        IsRead = false,
                //        Message = "Trust fund seitan chia, wolf lomo letterpress Bushwick before they sold out. Carles kogi fixie, squid twee Tonx readymade cred typewriter scenester locavore kale chips vegan organic. Meggings pug wolf Shoreditch typewriter skateboard. McSweeney's iPhone chillwave, food truck direct trade disrupt flannel irony tousled Cosby sweater single-origin coffee. Organic disrupt bicycle rights, tattooed messenger bag flannel craft beer fashion axe bitters. Readymade sartorial craft beer, quinoa sustainable butcher Marfa Echo Park Terry Richardson gluten-free flannel retro cred mlkshk banjo. Salvia 90's art party Blue Bottle, PBR&B cardigan 8-bit."
                //    };
                //    db.InsertNotification(notification);
                //    Console.WriteLine($"Notification {i} inserted.");
                //}

                //for (int i = 0; i < 100; i++)
                //{
                //    var createdOn = DateTime.Now.AddDays(-i);
                //    var record = new Record
                //    {
                //        Name = "My record " + i,
                //        Approved = false,
                //        AssignedOn = null,
                //        AssignedTo = null,
                //        Comments = "Trust fund seitan chia, wolf lomo letterpress Bushwick before they sold out. Carles kogi fixie, squid twee Tonx readymade cred typewriter scenester locavore kale chips vegan organic. Meggings pug wolf Shoreditch typewriter skateboard. McSweeney's iPhone chillwave, food truck direct trade disrupt flannel irony tousled Cosby sweater single-origin coffee. Organic disrupt bicycle rights, tattooed messenger bag flannel craft beer fashion axe bitters. Readymade sartorial craft beer, quinoa sustainable butcher Marfa Echo Park Terry Richardson gluten-free flannel retro cred mlkshk banjo. Salvia 90's art party Blue Bottle, PBR&B cardigan 8-bit.",
                //        ManagerComments = "Trust fund seitan chia, wolf lomo letterpress Bushwick before they sold out. Carles kogi fixie, squid twee Tonx readymade cred typewriter scenester locavore kale chips vegan organic. Meggings pug wolf Shoreditch typewriter skateboard. McSweeney's iPhone chillwave, food truck direct trade disrupt flannel irony tousled Cosby sweater single-origin coffee. Organic disrupt bicycle rights, tattooed messenger bag flannel craft beer fashion axe bitters. Readymade sartorial craft beer, quinoa sustainable butcher Marfa Echo Park Terry Richardson gluten-free flannel retro cred mlkshk banjo. Salvia 90's art party Blue Bottle, PBR&B cardigan 8-bit.",
                //        CreatedBy = "1",
                //        Description = "Trust fund seitan chia, wolf lomo letterpress Bushwick before they sold out. Carles kogi fixie, squid twee Tonx readymade cred typewriter scenester locavore kale chips vegan organic. Meggings pug wolf Shoreditch typewriter skateboard. McSweeney's iPhone chillwave, food truck direct trade disrupt flannel irony tousled Cosby sweater single-origin coffee. Organic disrupt bicycle rights, tattooed messenger bag flannel craft beer fashion axe bitters. Readymade sartorial craft beer, quinoa sustainable butcher Marfa Echo Park Terry Richardson gluten-free flannel retro cred mlkshk banjo. Salvia 90's art party Blue Bottle, PBR&B cardigan 8-bit.",
                //        StartDate = createdOn,
                //        EndDate = DateTime.Now.AddDays(i),
                //        ModifiedBy = null,
                //        ModifiedOn = null,
                //    };
                //    db.InsertRecord(record);
                //    Console.WriteLine($"Reocrd {i} inserted.");
                //}

                db.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }

            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
