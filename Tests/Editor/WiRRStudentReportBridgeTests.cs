using System;
using System.Collections.Generic;
using System.Reflection;
using KIA.WiRR;
using NUnit.Framework;

namespace KIA.WiRR.Editor.Tests
{
    public sealed class WiRRStudentReportBridgeTests
    {
        private static void Validate(WiRRReportDocument document, string[] indices, string group)
        {
            var type = typeof(global::WiRRPublicReportSubmission).Assembly.GetType(
                "KIA.WiRR.Editor.WiRRStudentReportBridge", true);
            var method = type.GetMethod("ValidateExistingOwner", BindingFlags.NonPublic | BindingFlags.Static);
            try { method.Invoke(null, new object[] { document, indices, group }); }
            catch (TargetInvocationException error) { throw error.InnerException; }
        }

        private static object Invoke(string methodName, params object[] args)
        {
            var type = typeof(global::WiRRPublicReportSubmission).Assembly.GetType(
                "KIA.WiRR.Editor.WiRRStudentReportBridge", true);
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            try { return method.Invoke(null, args); }
            catch (TargetInvocationException error) { throw error.InnerException; }
        }

        [TestCase(1, 1, 2)]
        [TestCase(2, 3, 4)]
        [TestCase(3, 5, 6)]
        [TestCase(4, 7, 8)]
        public void AllBlocksMapToTwoDistinctReports(int block, int first, int second)
        {
            CollectionAssert.AreEqual(new[] { first, second }, (int[])Invoke("BlockLabs", block));
        }

        [TestCase(0)]
        [TestCase(5)]
        public void InvalidBlockIsRejectedBeforeDraftMutation(int block)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Invoke("BlockLabs", block));
        }

        [TestCase(1, "DESKTOP_SIM")]
        [TestCase(2, "DESKTOP_SIM")]
        [TestCase(3, "SIM_AR")]
        [TestCase(4, "SIM_AR")]
        [TestCase(5, "DESKTOP_SIM")]
        [TestCase(6, "ROS_REPLAY")]
        [TestCase(7, "DESKTOP_SIM")]
        [TestCase(8, "DESKTOP_SIM")]
        public void ExpectedSourceIsExplicitAndDoesNotClaimRosLive(int lab, string expected)
        {
            Assert.AreEqual(expected, (string)Invoke("ExpectedMode", lab));
        }

        [TestCase(0)]
        [TestCase(9)]
        public void InvalidLabHasNoSourceMode(int lab)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Invoke("ExpectedMode", lab));
        }

        [Test]
        public void ForeignDraftWithoutTeamIdIsRejectedWithoutRelabelling()
        {
            var document = new WiRRReportDocument {
                group = "G1", studentIndices = new List<string> { "999992" }
            };
            Assert.Throws<ArgumentException>(() => Validate(document, new[] { "999991" }, "G1"));
            Assert.AreEqual("999992", document.studentIndices[0]);
        }

        [Test]
        public void PairCanBeImportedByEitherMemberWithoutChangingTheDraft()
        {
            var document = new WiRRReportDocument {
                group = "G1", studentIndices = new List<string> { "999992", "999991" }
            };
            Assert.DoesNotThrow(() => Validate(document, new[] { "999991", "999992" }, "G1"));
            CollectionAssert.AreEqual(new[] { "999992", "999991" }, document.studentIndices);
        }

        [Test]
        public void ForeignGroupIsRejectedBeforeWriting()
        {
            var document = new WiRRReportDocument {
                group = "G2", studentIndices = new List<string> { "999991" }
            };
            Assert.Throws<ArgumentException>(() => Validate(document, new[] { "999991" }, "G1"));
            Assert.AreEqual("G2", document.group);
        }
    }
}
