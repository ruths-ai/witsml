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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Energistics.DataAccess.WITSML141;
using PDS.Framework;

namespace PDS.Witsml.Server.Configuration
{
    /// <summary>
    /// CapServer141Provider tests.
    /// </summary>
    [TestClass]
    public class CapServer141ProviderTests
    {
        private DevKit141Aspect _devKit;
        private CapServer141Provider _capServer141Provider;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestSetUp()
        {
            _devKit = new DevKit141Aspect(TestContext);
            _devKit.Store.CapServerProviders = _devKit.Store.CapServerProviders
                .Where(x => x.DataSchemaVersion == OptionsIn.DataVersion.Version141.Value)
                .ToArray();

            _capServer141Provider = _devKit.Store.CapServerProviders.FirstOrDefault() as CapServer141Provider;
        }

        [TestMethod]
        public void CapServer141Provider_DataSchemaVersion_Can_Get_DataVersion()
        {
            Assert.IsNotNull(_capServer141Provider);
            Assert.AreEqual("1.4.1.1", _capServer141Provider.DataSchemaVersion);
        }

        [TestMethod]
        public void CapServer141Provider_Providers_Can_Get_Providers()
        {
            Assert.IsNotNull(_capServer141Provider.Providers);
        }

        [TestMethod]
        public void CapServer141Provider_ToXml_Can_Get_Server_Capabilities_As_Xml()
        {
            var capServerXml = _capServer141Provider.ToXml();

            Assert.IsTrue(capServerXml != string.Empty);

            var capServerObject = Energistics.DataAccess.EnergisticsConverter.XmlToObject<CapServers>(capServerXml).CapServer;

            Assert.AreEqual("1.4.1", capServerObject.ApiVers);
            Assert.AreEqual(Properties.Settings.Default.DefaultServerName, capServerObject.Name, "Server Name");
            Assert.AreEqual(Properties.Settings.Default.DefaultVendorName, capServerObject.Vendor, "Vendor");
            Assert.AreEqual(Properties.Settings.Default.DefaultServerDescription, capServerObject.Description, "Server Description");
            Assert.AreEqual("1.4.1.1", capServerObject.SchemaVersion, "Schema Version");
            Assert.AreEqual(Properties.Settings.Default.DefaultContactName, capServerObject.Contact.Name, "Contact Name");
            Assert.AreEqual(Properties.Settings.Default.DefaultContactEmail, capServerObject.Contact.Email, "Contact Email");
            Assert.AreEqual(Properties.Settings.Default.DefaultContactPhone, capServerObject.Contact.Phone, "Contact Phone");

            Assert.IsNotNull(capServerObject.MaxRequestLatestValues);
            Assert.AreEqual(Properties.Settings.Default.MaxRequestLatestValues, capServerObject.MaxRequestLatestValues.Value, "maxRequestLatestValue");
            Assert.IsNotNull(capServerObject.SupportUomConversion);
            Assert.IsTrue(!string.IsNullOrEmpty(capServerObject.CompressionMethod));

            Assert.AreEqual(4, capServerObject.Function.Count, "Server Functions");
        }

        [TestMethod]
        public void CapServer141Provider_ToXml_Can_Get_Server_Capabilities_For_GetFromStore_With_Object_Contraints_For_GrowingObjects()
        {
            var capServerObject = GetCapServerObject();

            var getFromStore = capServerObject.Function.Where(n => n.Name.EndsWith(Functions.GetFromStore.ToString())).ToArray();
            Assert.IsNotNull(getFromStore);

            getFromStore.FirstOrDefault()?.DataObject.ForEach(
                    dataObject =>
                    {
                        if (ObjectTypes.IsGrowingDataObject(dataObject.Value))
                        {
                            if (dataObject.Value == ObjectTypes.Log)
                                Assert.AreEqual(Properties.Settings.Default.LogMaxDataPointsGet, dataObject.MaxDataPoints, "MaxDataPoints");

                            var propertyNameMaxDataNodeGet = dataObject.Value.ToPascalCase() + "MaxDataNodesGet";
                            Assert.AreEqual(Properties.Settings.Default[propertyNameMaxDataNodeGet], dataObject.MaxDataNodes, propertyNameMaxDataNodeGet);
                        }
                    });
        }

        [TestMethod]
        public void CapServer141Provider_ToXml_Can_Get_Server_Capabilities_For_AddToStore_With_Object_Contraints_For_GrowingObjects()
        {
            var capServerObject = GetCapServerObject();

            var addToStore = capServerObject.Function.Where(n => n.Name.EndsWith(Functions.AddToStore.ToString())).ToArray();
            Assert.IsNotNull(addToStore);

            addToStore.FirstOrDefault()?.DataObject.ForEach(
                    dataObject =>
                    {
                        if (ObjectTypes.IsGrowingDataObject(dataObject.Value))
                        {
                            if (dataObject.Value == ObjectTypes.Log)
                                Assert.AreEqual(Properties.Settings.Default.LogMaxDataPointsAdd, dataObject.MaxDataPoints, "MaxDataPoints");

                            var propertyNameMaxDataNodeAdd = dataObject.Value.ToPascalCase() + "MaxDataNodesAdd";
                            Assert.AreEqual(Properties.Settings.Default[propertyNameMaxDataNodeAdd], dataObject.MaxDataNodes, propertyNameMaxDataNodeAdd);
                        }
                    });
        }

        [TestMethod]
        public void CapServer141Provider_ToXml_Can_Get_Server_Capabilities_For_UpdateInStore_With_Object_Contraints_For_GrowingObjects()
        {
            var capServerObject = GetCapServerObject();

            var updateInStore = capServerObject.Function.Where(n => n.Name.EndsWith(Functions.UpdateInStore.ToString())).ToArray();
            Assert.IsNotNull(updateInStore);

            updateInStore.FirstOrDefault()?.DataObject.ForEach(
                    dataObject =>
                    {
                        if (ObjectTypes.IsGrowingDataObject(dataObject.Value))
                        {
                            if (dataObject.Value == ObjectTypes.Log)
                                Assert.AreEqual(Properties.Settings.Default.LogMaxDataPointsUpdate, dataObject.MaxDataPoints, "MaxDataPoints");

                            var propertyNameMaxDataNodeUpdate = dataObject.Value.ToPascalCase() + "MaxDataNodesUpdate";
                            Assert.AreEqual(Properties.Settings.Default[propertyNameMaxDataNodeUpdate], dataObject.MaxDataNodes, propertyNameMaxDataNodeUpdate);
                        }
                    });
        }

        [TestMethod]
        public void CapServer141Provider_ToXml_Can_Get_Server_Capabilities_For_DeleteFromStore_With_Object_Contraints_For_GrowingObjects()
        {
            var capServerObject = GetCapServerObject();

            var deleteFromStore = capServerObject.Function.Where(n => n.Name.EndsWith(Functions.DeleteFromStore.ToString())).ToArray();
            Assert.IsNotNull(deleteFromStore);

            deleteFromStore.FirstOrDefault()?.DataObject.ForEach(
                    dataObject =>
                    {
                        if (ObjectTypes.IsGrowingDataObject(dataObject.Value))
                        {
                            if (dataObject.Value == ObjectTypes.Log)
                                Assert.AreEqual(Properties.Settings.Default.LogMaxDataPointsDelete, dataObject.MaxDataPoints, "MaxDataPoints");

                            var propertyNameMaxDataNodeDelete = dataObject.Value.ToPascalCase() + "MaxDataNodesDelete";
                            Assert.AreEqual(Properties.Settings.Default[propertyNameMaxDataNodeDelete], dataObject.MaxDataNodes, propertyNameMaxDataNodeDelete);
                        }
                    });
        }

        [TestMethod]
        public void CapServer141Provider_IsSupported_AddToStore_Can_Check_Supported_Object_Type()
        {
            var capServerObject = GetCapServerObject();

            var addToStore = capServerObject.Function.Where(n => n.Name.EndsWith(Functions.AddToStore.ToString())).ToArray();
            Assert.IsNotNull(addToStore);

            addToStore.FirstOrDefault()?.DataObject.ForEach(
                    dataObject =>
                    {
                        Assert.IsTrue(_capServer141Provider.IsSupported(Functions.AddToStore, dataObject.Value), dataObject.Value);
                    });

            Assert.IsFalse(_capServer141Provider.IsSupported(Functions.AddToStore, ObjectTypes.Unknown), ObjectTypes.Unknown);
        }

        [TestMethod]
        public void CapServer141Provider_IsSupported_GetFromStore_Can_Check_Supported_Object_Type()
        {
            var capServerObject = GetCapServerObject();

            var getFromStore = capServerObject.Function.Where(n => n.Name.EndsWith(Functions.GetFromStore.ToString())).ToArray();
            Assert.IsNotNull(getFromStore);

            getFromStore.FirstOrDefault()?.DataObject.ForEach(
                    dataObject =>
                    {
                        Assert.IsTrue(_capServer141Provider.IsSupported(Functions.GetFromStore, dataObject.Value), dataObject.Value);
                    });

            Assert.IsFalse(_capServer141Provider.IsSupported(Functions.GetFromStore, ObjectTypes.Unknown), ObjectTypes.Unknown);
        }

        [TestMethod]
        public void CapServer141Provider_IsSupported_UpdateInStore_Can_Check_Supported_Object_Type()
        {
            var capServerObject = GetCapServerObject();

            var updateInStore = capServerObject.Function.Where(n => n.Name.EndsWith(Functions.UpdateInStore.ToString())).ToArray();
            Assert.IsNotNull(updateInStore);

            updateInStore.FirstOrDefault()?.DataObject.ForEach(
                    dataObject =>
                    {
                        Assert.IsTrue(_capServer141Provider.IsSupported(Functions.UpdateInStore, dataObject.Value), dataObject.Value);
                    });

            Assert.IsFalse(_capServer141Provider.IsSupported(Functions.UpdateInStore, ObjectTypes.Unknown), ObjectTypes.Unknown);
        }

        [TestMethod]
        public void CapServer141Provider_IsSupported_DeleteFromStore_Can_Check_Supported_Object_Type()
        {
            var capServerObject = GetCapServerObject();

            var deleteFromStore = capServerObject.Function.Where(n => n.Name.EndsWith(Functions.DeleteFromStore.ToString())).ToArray();
            Assert.IsNotNull(deleteFromStore);

            deleteFromStore.FirstOrDefault()?.DataObject.ForEach(
                    dataObject =>
                    {
                        Assert.IsTrue(_capServer141Provider.IsSupported(Functions.DeleteFromStore, dataObject.Value), dataObject.Value);
                    });

            Assert.IsFalse(_capServer141Provider.IsSupported(Functions.DeleteFromStore, ObjectTypes.Unknown), ObjectTypes.Unknown);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            _devKit = null;
        }

        private CapServer GetCapServerObject()
        {
            var capServerXml = _capServer141Provider.ToXml();

            return Energistics.DataAccess.EnergisticsConverter.XmlToObject<CapServers>(capServerXml).CapServer;
        }
    }
}
