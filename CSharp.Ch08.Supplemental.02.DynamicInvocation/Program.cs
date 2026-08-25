#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using System;
using CSharp.Ch08.Supplemental._02.DynamicInvocation.HelperClasses;
using CSharp.Ch08.Supplemental._02.DynamicInvocation.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch08.Supplemental._02.DynamicInvocation
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * The main Chapter 8 lesson demonstrated one narrow slice of "calling code you don't
         *   have a compile-time reference to": invoking a no-argument method by name. This
         *   project rounds that out into the genuinely useful shape reflection takes in real
         *   code: creating objects dynamically, reading and writing their properties by name,
         *   calling methods that take real arguments, and (the payoff) a small, reusable
         *   utility built entirely out of these pieces.
         *
         * - Activator.CreateInstance<T>()     Create an instance when you know the type at
         *                                        compile time but still want to go through
         *                                        Activator (rare; usually just "new T()" is
         *                                        simpler when you can write it)
         * - Activator.CreateInstance(Type)     Create an instance when you only have a Type
         *                                        object, not a compile-time type name, the
         *                                        actually common case
         * - Activator.CreateInstance(Type, args)  Same, but calling a specific constructor
         *                                        overload by passing constructor arguments
         * - PropertyInfo.GetValue()/SetValue() Read/write a property's value by name, on any
         *                                        object, without a compile-time reference to
         *                                        that property
         * - MethodInfo.Invoke(target, args)    Call a method by name, passing real arguments,
         *                                        not just the no-argument case shown earlier
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // Demonstrate Activator.CreateInstance() in its various forms
                UsingActivatorCreateInstance();
                GenericFunctions.Pause();

                // Demonstrate PropertyInfo.GetValue()/SetValue()
                UsingPropertyGetAndSetValue();
                GenericFunctions.Pause();

                // Demonstrate invoking a method that takes real parameters
                InvokingMethodWithParameters();
                GenericFunctions.Pause();

                // The payoff: a small, reusable, reflection-based property mapper
                UsingPropertyMapper();
                GenericFunctions.Pause();
                #endregion
            }
            catch (Exception ex)
            {
                new DatabankException("Error Caught!", ex).Log();
                GenericFunctions.Pause();
            }
            finally
            {
                GenericFunctions.Pause(final: true);
            }
        }
        #endregion

        #region Lesson Methods
        // Demonstrate Activator.CreateInstance() in its various forms
        private static void UsingActivatorCreateInstance()
        {
            // Generic form: you know the type at compile time. Rarely actually needed, since
            //   "new Product()" does the same thing more simply, shown here for comparison.
            var viaGeneric = Activator.CreateInstance<Product>();
            Console.WriteLine($"Activator.CreateInstance<Product>(): Id={viaGeneric.Id}, Name={viaGeneric.Name ?? "(null)"}");

            // Non-generic form: you only have a Type object, the genuinely common case, e.g.
            //   when the type was itself looked up by name (see the main lesson's
            //   Assembly.CreateInstance(), which works similarly).
            var productType = typeof(Product);
            var viaType = (Product)Activator.CreateInstance(productType);
            Console.WriteLine($"Activator.CreateInstance(typeof(Product)): Id={viaType.Id}, Name={viaType.Name ?? "(null)"}");

            // With constructor arguments: selects and calls the matching constructor overload.
            var viaTypeWithArgs = (Product)Activator.CreateInstance(productType, 1, "Widget", 9.99m);
            Console.WriteLine($"Activator.CreateInstance(typeof(Product), 1, \"Widget\", 9.99m): " +
                               $"Id={viaTypeWithArgs.Id}, Name={viaTypeWithArgs.Name}, Price={viaTypeWithArgs.Price:C}");
        }

        // Demonstrate PropertyInfo.GetValue()/SetValue()
        private static void UsingPropertyGetAndSetValue()
        {
            var product = new Product();
            var productType = typeof(Product);

            // Set properties purely by name, as strings, no compile-time reference to
            //   Product.Name or Product.Price anywhere in this method.
            productType.GetProperty("Name")?.SetValue(product, "Gadget");
            productType.GetProperty("Price")?.SetValue(product, 24.99m);

            // Read them back the same way
            var name = productType.GetProperty("Name")?.GetValue(product);
            var price = productType.GetProperty("Price")?.GetValue(product);

            Console.WriteLine($"Set via reflection, then read back via reflection: Name={name}, Price={price:C}");

            // Confirm this actually changed the real object, not just some reflection-only copy
            Console.WriteLine($"Same values via a normal compile-time reference: Name={product.Name}, Price={product.Price:C}");
        }

        // Demonstrate invoking a method that takes real parameters
        private static void InvokingMethodWithParameters()
        {
            var product = new Product(2, "Widget", 100m);
            var productType = typeof(Product);

            var applyDiscountMethod = productType.GetMethod("ApplyDiscount");

            // Unlike the main lesson's parameterless Invoke(ta, null), this passes a real
            //   argument array, one entry per parameter, in declaration order.
            var discountedPrice = applyDiscountMethod?.Invoke(product, [0.25m]);

            Console.WriteLine($"product.ApplyDiscount(0.25m) invoked via reflection: {discountedPrice:C}");
            Console.WriteLine($"(original Price is unchanged, ApplyDiscount() returns a new value rather than mutating): {product.Price:C}");
        }

        // The payoff: a small, reusable, reflection-based property mapper
        private static void UsingPropertyMapper()
        {
            var product = new Product(3, "Thingamajig", 49.99m);
            var dto = new ProductDto { Source = "Imported from legacy system" };

            Console.WriteLine("Before mapping:");
            Console.WriteLine($" - dto.Id={dto.Id}, dto.Name={dto.Name ?? "(null)"}, dto.Price={dto.Price:C}, dto.Source={dto.Source}");

            PropertyMapper.CopyMatchingProperties(product, dto);

            Console.WriteLine($"{Environment.NewLine}After mapping matching properties from product onto dto:");
            Console.WriteLine($" - dto.Id={dto.Id}, dto.Name={dto.Name}, dto.Price={dto.Price:C}, dto.Source={dto.Source}");
            Console.WriteLine($"{Environment.NewLine}Note: dto.Source was left alone, Product has no Source property to copy from.");
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
