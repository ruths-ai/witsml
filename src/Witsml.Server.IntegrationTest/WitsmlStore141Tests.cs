﻿using System.Linq;
using Energistics.DataAccess;
using Energistics.DataAccess.WITSML141;
using Energistics.DataAccess.WITSML141.ComponentSchemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDS.Framework;
using PDS.Witsml.Server.Data;
using PDS.Witsml.Server.Data.Wellbores;

namespace PDS.Witsml.Server
{
    [TestClass]
    public class WitsmlStore141Tests
    {
        private static readonly DevKit141Aspect DevKit = new DevKit141Aspect(null);

        [TestInitialize]
        public void TestSetUp()
        {
        }

        [TestMethod]
        public void Can_get_version()
        {
            var request = new WMLS_GetVersionRequest();
            var response = DevKit.Store.WMLS_GetVersion(request);

            Assert.IsNotNull(response);
            if (!string.IsNullOrEmpty(response.Result))
            {
                var versions = response.Result.Split(',');
                Assert.IsNotNull(versions);
                Assert.IsTrue(versions.Length > 0);
                foreach (var version in versions)
                    Assert.IsFalse(string.IsNullOrEmpty(version));
            }
        }

        [TestMethod]
        public void Version_order_oldest_first()
        {
            var request = new WMLS_GetVersionRequest();
            var response = DevKit.Store.WMLS_GetVersion(request);

            Assert.IsNotNull(response);
            var ordered = true;
            if (!string.IsNullOrEmpty(response.Result))
            {
                var versions = response.Result.Split(',');
                Assert.IsNotNull(versions);
                Assert.IsTrue(versions.Length > 0);
                var version = versions[0];
                Assert.IsFalse(string.IsNullOrEmpty(version));
                for (var i = 1; i < versions.Length; i++)
                {
                    if (string.Compare(version, versions[i]) >= 0)
                    {
                        ordered = false;
                        break;
                    }
                    version = versions[i];
                }
            }

            Assert.IsTrue(ordered);
        }

        [TestMethod]
        public void Can_add_well_without_validation()
        {
            var well = new Well { Name = "Well-to-add-01" };
            var response = DevKit.AddWell(well);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);
        }

        [TestMethod]
        public void Adding_duplicate_well_uid_causes_database_error()
        {
            var well = new Well { Name = "Well-to-test-add-error", Uid = DevKit.Uid() };
            var response = DevKit.AddWell(well);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            response = DevKit.AddWell(well);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.ErrorAddingToDataStore, response.Result);
        }

        [TestMethod]
        public void Uid_returned_add_well()
        {
            var well = new Well { Name = "Well-to-add-01" };
            var response = DevKit.AddWell(well);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uid = response.SuppMsgOut;
            var valid = !string.IsNullOrEmpty(uid);
            Assert.IsTrue(valid);

            well = new Well { Uid = uid };
            var result = DevKit.QueryWell(well);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            well = result.FirstOrDefault();
            Assert.IsNotNull(well);
            Assert.AreEqual(uid, well.Uid);
        }

        [TestMethod]
        public void Case_preserved_add_well()
        {
            var nameLegal = "Well Legal Name";
            var well = new Well { Name = "Well-to-add-01", NameLegal = nameLegal };
            var response = DevKit.AddWell(well);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var uid = response.SuppMsgOut;
            var valid = !string.IsNullOrEmpty(uid);
            Assert.IsTrue(valid);

            well = new Well { Uid = uid };
            var result = DevKit.QueryWell(well);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            well = result.FirstOrDefault();
            Assert.IsNotNull(well);
            Assert.AreEqual(nameLegal, well.NameLegal);
        }

        [TestMethod]
        public void Can_add_wellbore_without_validation()
        {
            var well = new Well { Name = "Well-to-add-01" };
            var response = DevKit.AddWell(well);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var wellbore = new Wellbore { Name = "Wellbore-to-add-01", NameWell = well.Name, UidWell = response.SuppMsgOut };
            response = DevKit.AddWellbore(wellbore);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);
        }

        [TestMethod]
        public void Adding_wellbore_database_configuration_error()
        {
            var well = new Well { Name = "Well-to-add-02" };
            var response = DevKit.AddWell(well);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var dbProvider = new TestDatabaseProvider(new MongoDbClassMapper(), string.Empty);
            var wellboreAdapter = new Wellbore141DataAdapter(dbProvider);
            wellboreAdapter.Container = ContainerFactory.Create();

            var caught = false;
            WitsmlException exception = null;

            try
            {
                var wellbore = new Wellbore { Name = "Wellbore-to-test-add-error", NameWell = well.Name, UidWell = response.SuppMsgOut };
                wellboreAdapter.Add(wellbore);
            }
            catch (WitsmlException ex)
            {
                caught = true;
                exception = ex;
            }

            Assert.IsTrue(caught);
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.ErrorAddingToDataStore, exception.ErrorCode);
        }

        [TestMethod]
        public void Test_error_code_401_missing_plural_root_element_xmlIn()
        {
            var well = new Well { Name = "Well-to-add-missing-plural-root" };
            var xmlIn = EnergisticsConverter.ObjectToXml(well);
            var response = DevKit.AddToStore(ObjectTypes.Well, xmlIn, null, null);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.MissingPluralRootElement, response.Result);
        }

        [TestMethod]
        public void Test_error_code_404_invalid_schema_version()
        {
            var client = new CapClient { ApiVers = "1.4.1.1", SchemaVersion = "1.4.1.1,1.3.1.1" };
            var clients = new CapClients { Version = "1.4.1.1", CapClient = client };
            var capabilitiesIn = EnergisticsConverter.ObjectToXml(clients);
            var well = new Well { Name = "Well-to-add-invalid-schema-version" };
            var response = DevKit.AddWell(well, capClient: capabilitiesIn);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.InvalidClientSchemaVersion, response.Result);
        }

        [TestMethod]
        public void Test_error_code_407_missing_witsml_object_type()
        {
            var well = new Well { Name = "Well-to-add-missing-witsml-type" };
            var response = DevKit.AddWell(well, string.Empty);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.MissingWMLtypeIn, response.Result);
        }

        [TestMethod]
        public void Test_error_code_408_missing_input_template()
        {
            var response = DevKit.AddToStore(ObjectTypes.Well, null, null, null);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.MissingInputTemplate, response.Result);
        }

        [TestMethod]
        public void Test_error_code_409_non_conforming_input_template()
        {
            var well = new Well { Name = "Well-to-add-invalid-input-template" };
            var response = DevKit.AddWell(well);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.InputTemplateNonConforming, response.Result);
        }

        [TestMethod]
        public void Test_error_code_411_optionsIn_invalid_format()
        {
            var well = new Well { Name = "Well-to-add-invalid-optionsIn-format" };
            var response = DevKit.AddWell(well, optionsIn: "compressionMethod:gzip");

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.ParametersNotEncodedByRules, response.Result);
        }

        [TestMethod]
        public void Test_error_code_413_unsupported_data_object()
        {
            var well = new Well { Name = "Well-to-add-unsupported-error" };
            var wells = new WellList { Well = DevKit.List(well) };

            // update Version property to an unsupported data schema version
            wells.Version = "1.4.x.y";

            var xmlIn = EnergisticsConverter.ObjectToXml(wells);
            var response = DevKit.AddToStore(ObjectTypes.Well, xmlIn, null, null);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.DataObjectNotSupported, response.Result);
        }

        [TestMethod]
        public void Test_error_code_440_optionsIn_keyword_not_recognized()
        {
            var well = new Well { Name = "Well-to-add-invalid-optionsIn-keyword" };
            var response = DevKit.AddWell(well, optionsIn: "returnElements=all");

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.KeywordNotSupportedByFunction, response.Result);
        }

        [TestMethod]
        public void Test_error_code_441_optionsIn_value_not_recognized()
        {
            var well = new Well { Name = "Well-to-add-invalid-optionsIn-value" };
            var response = DevKit.AddWell(well, optionsIn: "compressionMethod=7zip");

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.InvalidKeywordValue, response.Result);
        }

        [TestMethod]
        public void Test_error_code_442_optionsIn_keyword_not_supported()
        {
            var well = new Well { Name = "Well-to-add-optionsIn-keyword-not-supported" };
            var response = DevKit.AddWell(well, optionsIn: "intervalRangeInclusion=any-part");

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.KeywordNotSupportedByServer, response.Result);
        }

        [TestMethod]
        public void Test_error_code_444_mulitple_data_objects_error()
        {
            var well1 = new Well { Name = "Well-to-01", Uid = DevKit.Uid() };
            var well2 = new Well { Name = "Well-to-02", Uid = DevKit.Uid() };
            var wells = new WellList { Well = DevKit.List(well1, well2) };

            var xmlIn = EnergisticsConverter.ObjectToXml(wells);
            var response = DevKit.AddToStore(ObjectTypes.Well, xmlIn, null, null);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.InputTemplateMultipleDataObjects, response.Result);
        }

        [TestMethod]
        public void Test_error_code_453_missing_unit_for_measure_data()
        {
            var well = new Well
            {
                Name = "Well-to-add-missing-unit",
                WellheadElevation = new WellElevationCoord { Value = 12.0 }
            };
            var response = DevKit.AddWell(well);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.MissingUnitForMeasureData, response.Result);
        }

        [TestMethod]
        public void Test_error_code_465_api_version_not_match()
        {
            var client = new CapClient { ApiVers = "1.3.1.1", SchemaVersion = "1.3.1.1" };
            var clients = new CapClients { Version = "1.4.1.1", CapClient = client };
            var capabilitiesIn = EnergisticsConverter.ObjectToXml(clients);
            var well = new Well { Name = "Well-to-add-apiVers-not-match" };
            var response = DevKit.AddWell(well, capClient: capabilitiesIn);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.ApiVersionNotMatch, response.Result);
        }

        [TestMethod]
        public void Test_error_code_466_non_conforming_capabilities_in()
        {
            var well = new Well { Name = "Well-to-add-invalid-capabilitiesIn" };
            var response = DevKit.AddWell(well, ObjectTypes.Well, "<capClients />");

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.CapabilitiesInNonConforming, response.Result);
        }

        [TestMethod]
        public void Test_error_code_467_unsupported_data_schema_version()
        {
            var client = new CapClient { ApiVers = "1.4.1.1"};
            var clients = new CapClients { Version = "1.4.x.y", CapClient = client };
            var capabilitiesIn = EnergisticsConverter.ObjectToXml(clients);
            var well = new Well { Name = "Well-to-add-unsupported-schema-version" };
            var response = DevKit.AddWell(well, capClient: capabilitiesIn);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.ApiVersionNotSupported, response.Result);
        }

        [TestMethod]
        public void Test_error_code_468_missing_version_attribute()
        {
            var well = new Well { Name = "Well-to-add-missing-version-attribute" };
            var wells = new WellList { Well = DevKit.List(well) };

            // update Version property to an unsupported data schema version
            wells.Version = null;

            var xmlIn = EnergisticsConverter.ObjectToXml(wells);
            var response = DevKit.AddToStore(ObjectTypes.Well, xmlIn, null, null);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.MissingDataSchemaVersion, response.Result);
        }

        [TestMethod]
        public void Test_error_code_473_schema_version_not_match()
        {
            var client = new CapClient { ApiVers = "1.4.1.1", SchemaVersion = "1.3.1.1" };
            var clients = new CapClients { Version = "1.4.1.1", CapClient = client };
            var capabilitiesIn = EnergisticsConverter.ObjectToXml(clients);
            var well = new Well { Name = "Well-to-add-schema-version-not-match" };
            var response = DevKit.AddWell(well, capClient: capabilitiesIn);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.SchemaVersionNotMatch, response.Result);
        }

        [TestMethod]
        public void Test_error_code_486_data_object_types_dont_match()
        {
            var well = new Well { Name = "Well-to-add-data-type-not-match" };
            var wells = new WellList { Well = DevKit.List(well) };

            var xmlIn = EnergisticsConverter.ObjectToXml(wells);
            var response = DevKit.AddToStore(ObjectTypes.Wellbore, xmlIn, null, null);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.DataObjectTypesDontMatch, response.Result);
        }

        [TestMethod]
        public void Test_error_code_487_data_object_not_supported()
        {
            var entity = new Target { Name = "Entity-to-test-unsupported-error" };
            var list = new TargetList { Target = DevKit.List(entity) };

            var xmlIn = EnergisticsConverter.ObjectToXml(list);
            var response = DevKit.AddToStore("target", xmlIn, null, null);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.DataObjectTypeNotSupported, response.Result);
        }
    }
}
