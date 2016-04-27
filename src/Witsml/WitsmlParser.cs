﻿//----------------------------------------------------------------------- 
// PDS.Witsml, 2016.1
//
// Copyright 2016 Petrotechnical Data Systems
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Energistics.DataAccess;
using PDS.Framework;

namespace PDS.Witsml
{
    /// <summary>
    /// Provides static helper methods that can be used to parse WITSML XML strings.
    /// </summary>
    public static class WitsmlParser
    {
        public static readonly XNamespace Xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");

        /// <summary>
        /// Parses the specified XML document using LINQ to XML.
        /// </summary>
        /// <param name="xml">The XML string.</param>
        /// <returns>An <see cref="XDocument"/> instance.</returns>
        /// <exception cref="WitsmlException"></exception>
        public static XDocument Parse(string xml)
        {
            try
            {
                return XDocument.Parse(xml);
            }
            catch (XmlException ex)
            {
                throw new WitsmlException(ErrorCodes.InputTemplateNonConforming, ex);
            }
        }

        /// <summary>
        /// Parses the specified XML document using the Standards DevKit.
        /// </summary>
        /// <typeparam name="T">The data object type.</typeparam>
        /// <param name="xml">The XML string.</param>
        /// <returns>The data object instance.</returns>
        /// <exception cref="WitsmlException"></exception>
        public static T Parse<T>(string xml)
        {
            try
            {
                return EnergisticsConverter.XmlToObject<T>(xml);
            }
            catch (Exception ex)
            {
                throw new WitsmlException(ErrorCodes.InputTemplateNonConforming, ex);
            }
        }

        /// <summary>
        /// Serialize WITSML query results to XML and remove empty elements and xsi:nil attributes.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The serialized XML string.</returns>
        public static string ToXml(object obj)
        {
            var xml = EnergisticsConverter.ObjectToXml(obj);
            var xmlDoc = Parse(xml);
            var root = xmlDoc.Root;

            foreach (var element in root.Elements())
            {
                RemoveEmptyElements(element);
            }

            return root.ToString();
        }

        /// <summary>
        /// Removes the empty descendant nodes from the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        public static void RemoveEmptyElements(XElement element)
        {
            Func<XElement, bool> predicate = e => e.Attributes(Xsi.GetName("nil")).Any() || 
                (string.IsNullOrEmpty(e.Value) && !e.HasAttributes && !e.HasElements);

            while (element.Descendants().Any(predicate))
            {
                element.Descendants().Where(predicate).Remove();
            }
        }

        /// <summary>
        /// Determines whether the specified element is a numeric type.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>True if it is a numeric type.</returns>
        public static bool IsNumericField(XElement element)
        {
            var elementPropertyName = char.ToUpper(element.Name.LocalName[0]) + element.Name.LocalName.Substring(1);
            var parentName = element.Parent.Name.LocalName;

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {                
                foreach (Type type in a.GetTypes())
                {
                    if (type.Name.EqualsIgnoreCase(parentName))
                    {
                        PropertyInfo propertyInfo = type.GetProperty(elementPropertyName);
                        Type propertyType = (propertyInfo != null) ? propertyInfo.PropertyType : null;

                        if (propertyType != null && propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {                           
                            propertyType = Nullable.GetUnderlyingType(propertyType);                            
                        }

                        if (propertyType == typeof(short) || propertyType == typeof(int) || propertyType == typeof(long) ||
                            propertyType == typeof(double) || propertyType == typeof(float) || propertyType == typeof(decimal))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Removes elements that are numeric type and have NaN value.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns>The xml with NaN removed.</returns>
        public static string RemoveNaNElements(string xml)
        {           
            Func<XElement, bool> predicate = e => e.Value.Equals("NaN") && IsNumericField(e);

            var xmlDoc = Parse(xml);
            var root = xmlDoc.Root;

            foreach (var element in root.Elements())
            {
                if (element.Descendants().Any(predicate))
                {
                    element.Descendants().Where(predicate).Remove();
                }
            }

            return xmlDoc.ToString();
        }
    }
}
