using System;
using FluentAssertions;
using NUnit.Framework;

namespace Atlassian.Jira.Test;

public class ComparableStringTest
{
    [Test]
    public void RefereceIsNull_EqualsOperators()
    {
        ComparableString field = null;
        (field == null).Should().BeTrue();
        (field != null).Should().BeFalse();
    }

    public class WithDate
    {
        [Test]
        public void StringEqualsOperator()
        {
            (new ComparableString("2012/05/01") == new DateTime(2012, 4, 1)).Should().BeFalse();
            (new ComparableString("2012/04/01") == new DateTime(2012, 4, 1)).Should().BeTrue();
        }

        [Test]
        public void StringNotEqualsOperator()
        {
            (new ComparableString("2012/05/01") != new DateTime(2012, 4, 1)).Should().BeTrue();
            (new ComparableString("2012/04/01") != new DateTime(2012, 4, 1)).Should().BeFalse();
        }

        [Test]
        public void StringGreaterThanOperator()
        {
            (new ComparableString("2012/01/10") > new DateTime(2012, 1, 1)).Should().BeTrue();
        }

        [Test]
        public void StringLessThanOperator()
        {
            (new ComparableString("2012/01/10") < new DateTime(2012, 1, 11)).Should().BeTrue();
        }

        [Test]
        public void StringLessThanOrEqualsOperator()
        {
            (new ComparableString("2012/01/10") <= new DateTime(2012, 1, 10)).Should().BeTrue();
        }
    }

    public class WithString
    {
        [Test]
        public void StringEqualsOperator()
        {
            (new ComparableString("bar") == "foo").Should().BeFalse();
            (new ComparableString("foo") == "foo").Should().BeTrue();
        }

        [Test]
        public void StringNotEqualsOperator()
        {
            (new ComparableString("bar") != "foo").Should().BeTrue();
            (new ComparableString("foo") != "foo").Should().BeFalse();
        }

        [Test]
        public void StringGreaterThanOperator()
        {
            (new ComparableString("TST-23") > "TST-1").Should().BeTrue();
        }

        [Test]
        public void StringLessThanOperator()
        {
            (new ComparableString("TST-1") < "TST-2").Should().BeTrue();
        }

        [Test]
        public void StringLessThanOrEqualsOperator()
        {
            (new ComparableString("TST-1") <= "TST-2").Should().BeTrue();
        }
    }

}
