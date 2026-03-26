using System;
using System.Collections.Generic;
using OutwardModsCommunicatorMenu.Utility.Parsing;
using UnityEngine;

namespace OutwardModsCommunicatorMenu.Tests
{
    public class TypeParserTests : ParsingTestBase
    {
        private readonly ValueParser _parser;

        public TypeParserTests()
        {
            _parser = new ValueParser();
        }

        public void RunAllTests()
        {
            TestPrimitives();
            TestNullables();
            TestCollections();
            TestVectors();
            TestEnums();
            ResultAggregator?.PrintSummary();
        }

        private void TestPrimitives()
        {
            LogTestStart("=== Primitive Type Tests ===");

            RunTest("Parse int", () =>
            {
                var (value, error) = _parser.TryParse("42", typeof(int));
                AssertEqual(42, value);
                AssertNull(error);
            });

            RunTest("Parse float", () =>
            {
                var (value, error) = _parser.TryParse("3.14", typeof(float));
                AssertEqual(3.14f, (float)value, 0.001f);
                AssertNull(error);
            });

            RunTest("Parse double", () =>
            {
                var (value, error) = _parser.TryParse("2.71828", typeof(double));
                AssertEqual(2.71828, (double)value, 0.0001);
                AssertNull(error);
            });

            RunTest("Parse bool true", () =>
            {
                var (value, error) = _parser.TryParse("true", typeof(bool));
                AssertEqual(true, value);
                AssertNull(error);
            });

            RunTest("Parse bool false", () =>
            {
                var (value, error) = _parser.TryParse("false", typeof(bool));
                AssertEqual(false, value);
                AssertNull(error);
            });

            RunTest("Parse string", () =>
            {
                var (value, error) = _parser.TryParse("hello world", typeof(string));
                AssertEqual("hello world", value);
                AssertNull(error);
            });

            RunTest("Parse long", () =>
            {
                var (value, error) = _parser.TryParse("9223372036854775807", typeof(long));
                AssertEqual(9223372036854775807L, value);
                AssertNull(error);
            });

            RunTest("Parse short", () =>
            {
                var (value, error) = _parser.TryParse("32767", typeof(short));
                AssertEqual((short)32767, value);
                AssertNull(error);
            });

            RunTest("Parse byte", () =>
            {
                var (value, error) = _parser.TryParse("255", typeof(byte));
                AssertEqual((byte)255, value);
                AssertNull(error);
            });

            RunTest("Parse char", () =>
            {
                var (value, error) = _parser.TryParse("A", typeof(char));
                AssertEqual('A', value);
                AssertNull(error);
            });

            LogTestEnd("=== Primitive Type Tests ===", true);
        }

        private void TestNullables()
        {
            LogTestStart("=== Nullable Type Tests ===");

            RunTest("Parse int?", () =>
            {
                var (value, error) = _parser.TryParse("42", typeof(int?));
                AssertEqual(42, value);
                AssertNull(error);
            });

            RunTest("Parse float?", () =>
            {
                var (value, error) = _parser.TryParse("3.14", typeof(float?));
                AssertEqual(3.14f, (float)value, 0.001f);
                AssertNull(error);
            });

            RunTest("Parse bool?", () =>
            {
                var (value, error) = _parser.TryParse("true", typeof(bool?));
                AssertEqual(true, value);
                AssertNull(error);
            });

            RunTest("Parse double?", () =>
            {
                var (value, error) = _parser.TryParse("1.5", typeof(double?));
                AssertEqual(1.5, (double)value, 0.001);
                AssertNull(error);
            });

            RunTest("Parse null int?", () =>
            {
                var (value, error) = _parser.TryParse("null", typeof(int?));
                AssertNull(value);
            });

            LogTestEnd("=== Nullable Type Tests ===", true);
        }

        private void TestCollections()
        {
            LogTestStart("=== Collection Type Tests ===");

            RunTest("Parse int[]", () =>
            {
                var (value, error) = _parser.TryParse("1 2 3", typeof(int[]));
                AssertNotNull(value);
                int[] arr = (int[])value;
                AssertEqual(3, arr.Length);
                AssertEqual(1, arr[0]);
                AssertEqual(2, arr[1]);
                AssertEqual(3, arr[2]);
                AssertNull(error);
            });

            RunTest("Parse string[]", () =>
            {
                var (value, error) = _parser.TryParse("item1 item2 item3", typeof(string[]));
                AssertNotNull(value);
                string[] arr = (string[])value;
                AssertEqual(3, arr.Length);
                AssertEqual("item1", arr[0]);
                AssertEqual("item2", arr[1]);
                AssertEqual("item3", arr[2]);
                AssertNull(error);
            });

            RunTest("Parse List<int>", () =>
            {
                var (value, error) = _parser.TryParse("10 20 30", typeof(List<int>));
                AssertNotNull(value);
                List<int> list = (List<int>)value;
                AssertEqual(3, list.Count);
                AssertEqual(10, list[0]);
                AssertEqual(20, list[1]);
                AssertEqual(30, list[2]);
                AssertNull(error);
            });

            RunTest("Parse List<string>", () =>
            {
                var (value, error) = _parser.TryParse("apple banana cherry", typeof(List<string>));
                AssertNotNull(value);
                List<string> list = (List<string>)value;
                AssertEqual(3, list.Count);
                AssertEqual("apple", list[0]);
                AssertEqual("banana", list[1]);
                AssertEqual("cherry", list[2]);
                AssertNull(error);
            });

            RunTest("Parse HashSet<int>", () =>
            {
                var (value, error) = _parser.TryParse("5 10 15", typeof(HashSet<int>));
                AssertNotNull(value);
                HashSet<int> set = (HashSet<int>)value;
                AssertEqual(3, set.Count);
                AssertBool(set.Contains(5));
                AssertBool(set.Contains(10));
                AssertBool(set.Contains(15));
                AssertNull(error);
            });

            RunTest("Parse float[]", () =>
            {
                var (value, error) = _parser.TryParse("1.1 2.2 3.3", typeof(float[]));
                AssertNotNull(value);
                float[] arr = (float[])value;
                AssertEqual(3, arr.Length);
                AssertEqual(1.1f, arr[0], 0.001f);
                AssertEqual(2.2f, arr[1], 0.001f);
                AssertEqual(3.3f, arr[2], 0.001f);
                AssertNull(error);
            });

            RunTest("Parse double[]", () =>
            {
                var (value, error) = _parser.TryParse("1.1 2.2 3.3", typeof(double[]));
                AssertNotNull(value);
                double[] arr = (double[])value;
                AssertEqual(3, arr.Length);
                AssertEqual(1.1, arr[0], 0.001);
                AssertEqual(2.2, arr[1], 0.001);
                AssertEqual(3.3, arr[2], 0.001);
                AssertNull(error);
            });

            RunTest("Parse bool[]", () =>
            {
                var (value, error) = _parser.TryParse("true false true", typeof(bool[]));
                AssertNotNull(value);
                bool[] arr = (bool[])value;
                AssertEqual(3, arr.Length);
                AssertEqual(true, arr[0]);
                AssertEqual(false, arr[1]);
                AssertEqual(true, arr[2]);
                AssertNull(error);
            });

            RunTest("Parse long[]", () =>
            {
                var (value, error) = _parser.TryParse("100 200 300", typeof(long[]));
                AssertNotNull(value);
                long[] arr = (long[])value;
                AssertEqual(3, arr.Length);
                AssertEqual(100L, arr[0]);
                AssertEqual(200L, arr[1]);
                AssertEqual(300L, arr[2]);
                AssertNull(error);
            });

            RunTest("Parse List<double>", () =>
            {
                var (value, error) = _parser.TryParse("1.5 2.5 3.5", typeof(List<double>));
                AssertNotNull(value);
                List<double> list = (List<double>)value;
                AssertEqual(3, list.Count);
                AssertEqual(1.5, list[0], 0.001);
                AssertEqual(2.5, list[1], 0.001);
                AssertEqual(3.5, list[2], 0.001);
                AssertNull(error);
            });

            RunTest("Parse HashSet<string>", () =>
            {
                var (value, error) = _parser.TryParse("apple banana apple", typeof(HashSet<string>));
                AssertNotNull(value);
                HashSet<string> set = (HashSet<string>)value;
                AssertEqual(2, set.Count);
                AssertBool(set.Contains("apple"));
                AssertBool(set.Contains("banana"));
                AssertNull(error);
            });

            LogTestEnd("=== Collection Type Tests ===", true);
        }

        private void TestVectors()
        {
            LogTestStart("=== Vector Type Tests ===");

            RunTest("Parse Vector2", () =>
            {
                var (value, error) = _parser.TryParse("1.5 2.5", typeof(Vector2));
                AssertNotNull(value);
                Vector2 v = (Vector2)value;
                AssertEqual(1.5f, v.x, 0.001f);
                AssertEqual(2.5f, v.y, 0.001f);
                AssertNull(error);
            });

            RunTest("Parse Vector3", () =>
            {
                var (value, error) = _parser.TryParse("1 2 3", typeof(Vector3));
                AssertNotNull(value);
                Vector3 v = (Vector3)value;
                AssertEqual(1f, v.x, 0.001f);
                AssertEqual(2f, v.y, 0.001f);
                AssertEqual(3f, v.z, 0.001f);
                AssertNull(error);
            });

            RunTest("Parse Vector4", () =>
            {
                var (value, error) = _parser.TryParse("1 2 3 4", typeof(Vector4));
                AssertNotNull(value);
                Vector4 v = (Vector4)value;
                AssertEqual(1f, v.x, 0.001f);
                AssertEqual(2f, v.y, 0.001f);
                AssertEqual(3f, v.z, 0.001f);
                AssertEqual(4f, v.w, 0.001f);
                AssertNull(error);
            });

            RunTest("Parse Quaternion", () =>
            {
                var (value, error) = _parser.TryParse("0 0 0 1", typeof(Quaternion));
                AssertNotNull(value);
                Quaternion q = (Quaternion)value;
                AssertEqual(0f, q.x, 0.001f);
                AssertEqual(0f, q.y, 0.001f);
                AssertEqual(0f, q.z, 0.001f);
                AssertEqual(1f, q.w, 0.001f);
                AssertNull(error);
            });

            RunTest("Parse Color", () =>
            {
                var (value, error) = _parser.TryParse("1 0 0 1", typeof(Color));
                AssertNotNull(value);
                Color c = (Color)value;
                AssertEqual(1f, c.r, 0.001f);
                AssertEqual(0f, c.g, 0.001f);
                AssertEqual(0f, c.b, 0.001f);
                AssertEqual(1f, c.a, 0.001f);
                AssertNull(error);
            });

            LogTestEnd("=== Vector Type Tests ===", true);
        }

        private void TestEnums()
        {
            LogTestStart("=== Enum Type Tests ===");

            RunTest("Parse ChatLogStatus", () =>
            {
                var (value, error) = _parser.TryParse("Success", typeof(OutwardModsCommunicatorMenu.Utility.Enums.ChatLogStatus));
                AssertEqual(OutwardModsCommunicatorMenu.Utility.Enums.ChatLogStatus.Success, value);
                AssertNull(error);
            });

            LogTestEnd("=== Enum Type Tests ===", true);
        }

        private void AssertEqual(float expected, float actual, float tolerance)
        {
            if (Math.Abs(expected - actual) > tolerance)
            {
                throw new Exception($"Assertion failed: expected {expected}, got {actual}");
            }
        }

        private void AssertEqual(double expected, double actual, double tolerance)
        {
            if (Math.Abs(expected - actual) > tolerance)
            {
                throw new Exception($"Assertion failed: expected {expected}, got {actual}");
            }
        }
    }
}
