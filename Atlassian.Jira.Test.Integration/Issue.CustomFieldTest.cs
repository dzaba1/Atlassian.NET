using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Atlassian.Jira.Test.Integration;

public class IssueCustomFieldTest
{
    private readonly Random _random = new Random();

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task CustomFieldsForProject_IfProjectDoesNotExist_ShouldThrowException(Jira jira)
    {
        var options = new CustomFieldFetchOptions();
        options.ProjectKeys.Add("FOO");
        Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await jira.Fields.GetCustomFieldsAsync(options).ToArrayAsync());

        Assert.Contains("Project with key 'FOO' was not found on the Jira server.", ex.Message);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task CustomFieldsForProject_ShouldReturnAllCustomFieldsOfAllIssueTypes(Jira jira)
    {
        var options = new CustomFieldFetchOptions();
        options.ProjectKeys.Add("TST");
        var results = await jira.Fields.GetCustomFieldsAsync(options).ToArrayAsync();
        Assert.Equal(21, results.Count());
    }

    /// <summary>
    /// Note that in the current data set all the custom fields are reused between the issue types.
    /// </summary>
    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task CustomFieldsForProjectAndIssueType_ShouldReturnAllCustomFieldsTheIssueType(Jira jira)
    {
        var options = new CustomFieldFetchOptions();
        options.ProjectKeys.Add("TST");
        options.IssueTypeNames.Add("Bug");

        var results = await jira.Fields.GetCustomFieldsAsync(options).ToArrayAsync();
        Assert.Equal(19, results.Count());
    }

    /// <summary>
    /// This case test the path when there are multiple custom fields defined in JIRA with the same name.
    /// Most likly because the user has a combination of Classic and NextGen projects. Since the test
    /// integration server is unable to create these type of custom fields, a property was added to the
    /// CustomFieldValueCollection that can force the new code path to execute.
    /// </summary>
    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task CanSetCustomFieldUsingSearchByProjectOnly(Jira jira)
    {
        var summaryValue = "Test issue with custom field by project" + _random.Next(int.MaxValue);
        var issue = new Issue(jira, "TST")
        {
            Type = "1",
            Summary = summaryValue,
            Assignee = "admin"
        };

        issue.CustomFields.SearchByProjectOnly = true;
        await issue.SetCustomFieldAsync("Custom Text Field", "My new value");
        await issue.SetCustomFieldAsync("Custom Date Field", "2015-10-03");

        var newIssue = await issue.SaveChangesAsync();

        Assert.Equal("My new value", await newIssue.GetCustomFieldAsync("Custom Text Field"));
        Assert.Equal("2015-10-03", await newIssue.GetCustomFieldAsync("Custom Date Field"));
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task AddAndReadCustomFieldById(Jira jira)
    {
        var summaryValue = "Test issue with custom text" + _random.Next(int.MaxValue);
        var issue = new Issue(jira, "TST")
        {
            Type = "1",
            Summary = summaryValue,
            Assignee = "admin"
        };

        issue.CustomFields.AddById("customfield_10000", "My Sample Text");
        await issue.SaveChangesAsync();

        var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value);
        Assert.Equal("My Sample Text", newIssue.CustomFields.First(f => f.Id.Equals("customfield_10000")).Values.First());
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task CreateIssueWithCascadingSelectFieldWithOnlyParentOptionSet(Jira jira)
    {
        var summaryValue = "Test issue with cascading select" + _random.Next(int.MaxValue);
        var issue = new Issue(jira, "TST")
        {
            Type = "1",
            Summary = summaryValue,
            Assignee = "admin"
        };

        // Add cascading select with only parent set.
        await issue.CustomFields.AddCascadingSelectFieldAsync("Custom Cascading Select Field", "Option3");
        await issue.SaveChangesAsync();

        var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value);

        var cascadingSelect = await newIssue.CustomFields.GetCascadingSelectFieldAsync("Custom Cascading Select Field");
        Assert.Equal("Option3", cascadingSelect.ParentOption);
        Assert.Null(cascadingSelect.ChildOption);
        Assert.Equal("Custom Cascading Select Field", cascadingSelect.Name);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task CreateAndQueryIssueWithLargeNumberCustomField(Jira jira)
    {
        var issue = new Issue(jira, "TST")
        {
            Type = "1",
            Summary = "Test issue with large custom field number" + _random.Next(int.MaxValue),
            Assignee = "admin"
        };

        await issue.SetCustomFieldAsync("Custom Number Field", "10000000000");
        await issue.SaveChangesAsync();

        var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value);
        Assert.Equal("10000000000", await newIssue.GetCustomFieldAsync("Custom Number Field"));
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task CreateAndQueryIssueWithComplexCustomFields(Jira jira)
    {
        var dateTime = new DateTime(2016, 11, 11, 11, 11, 0);
        var dateTimeStr = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz");
        dateTimeStr = dateTimeStr.Remove(dateTimeStr.LastIndexOf(":"), 1);

        var summaryValue = "Test issue with lots of custom fields (Created)" + _random.Next(int.MaxValue);
        var issue = new Issue(jira, "TST")
        {
            Type = "1",
            Summary = summaryValue,
            Assignee = "admin"
        };

        await issue.SetCustomFieldAsync("Custom Text Field", "My new value");
        await issue.SetCustomFieldAsync("Custom Date Field", "2015-10-03");
        await issue.SetCustomFieldAsync("Custom DateTime Field", dateTimeStr);
        await issue.SetCustomFieldAsync("Custom User Field", "admin");
        await issue.SetCustomFieldAsync("Custom Select Field", "Blue");
        await issue.SetCustomFieldAsync("Custom Group Field", "jira-users");
        await issue.SetCustomFieldAsync("Custom Project Field", "TST");
        await issue.SetCustomFieldAsync("Custom Version Field", "1.0");
        await issue.SetCustomFieldAsync("Custom Radio Field", "option1");
        await issue.SetCustomFieldAsync("Custom Number Field", "12.34");
        await issue.CustomFields.AddArrayAsync("Custom Labels Field", "label1", "label2");
        await issue.CustomFields.AddArrayAsync("Custom Multi Group Field", "jira-developers", "jira-users");
        await issue.CustomFields.AddArrayAsync("Custom Multi Select Field", "option1", "option2");
        await issue.CustomFields.AddArrayAsync("Custom Multi User Field", "admin", "test");
        await issue.CustomFields.AddArrayAsync("Custom Checkboxes Field", "option1", "option2");
        await issue.CustomFields.AddArrayAsync("Custom Multi Version Field", "2.0", "3.0");
        await issue.CustomFields.AddCascadingSelectFieldAsync("Custom Cascading Select Field", "Option2", "Option2.2");

        await issue.SaveChangesAsync();

        var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value);

        Assert.Equal("My new value", await newIssue.GetCustomFieldAsync("Custom Text Field"));
        Assert.Equal("2015-10-03", await newIssue.GetCustomFieldAsync("Custom Date Field"));
        Assert.Equal("admin", await newIssue.GetCustomFieldAsync("Custom User Field"));
        Assert.Equal("Blue", await newIssue.GetCustomFieldAsync("Custom Select Field"));
        Assert.Equal("jira-users", await newIssue.GetCustomFieldAsync("Custom Group Field"));
        Assert.Equal("TST", await newIssue.GetCustomFieldAsync("Custom Project Field"));
        Assert.Equal("1.0", await newIssue.GetCustomFieldAsync("Custom Version Field"));
        Assert.Equal("option1", await newIssue.GetCustomFieldAsync("Custom Radio Field"));
        Assert.Equal("12.34", await newIssue.GetCustomFieldAsync("Custom Number Field"));
        Assert.Equal("admin@example.com", (await newIssue.CustomFields.GetAsAsync<JiraUser>("Custom User Field")).Email);

        var serverDate = DateTime.Parse((await newIssue.GetCustomFieldAsync("Custom DateTime Field")).Value);
        Assert.Equal(dateTime, serverDate);

        Assert.Equal(new string[2] { "label1", "label2" }, (await newIssue.CustomFields.GetCustomFieldAsync("Custom Labels Field")).Values);
        Assert.Equal(new string[2] { "jira-developers", "jira-users" }, (await newIssue.CustomFields.GetCustomFieldAsync("Custom Multi Group Field")).Values);
        Assert.Equal(new string[2] { "option1", "option2" }, (await newIssue.CustomFields.GetCustomFieldAsync("Custom Multi Select Field")).Values);
        Assert.Equal(new string[2] { "admin", "test" }, (await newIssue.CustomFields.GetCustomFieldAsync("Custom Multi User Field")).Values);
        Assert.Equal(new string[2] { "option1", "option2" }, (await newIssue.CustomFields.GetCustomFieldAsync("Custom Checkboxes Field")).Values);
        Assert.Equal(new string[2] { "2.0", "3.0" }, (await newIssue.CustomFields.GetCustomFieldAsync("Custom Multi Version Field")).Values);

        var users = await newIssue.CustomFields.GetAsAsync<JiraUser[]>("Custom Multi User Field");
        Assert.Contains(users, u => u.Email == "test@qa.com");

        var cascadingSelect = await newIssue.CustomFields.GetCascadingSelectFieldAsync("Custom Cascading Select Field");
        Assert.Equal("Option2", cascadingSelect.ParentOption);
        Assert.Equal("Option2.2", cascadingSelect.ChildOption);
        Assert.Equal("Custom Cascading Select Field", cascadingSelect.Name);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task CanClearValueOfCustomField(Jira jira)
    {
        var summaryValue = "Test issue " + _random.Next(int.MaxValue);
        var issue = new Issue(jira, "TST")
        {
            Type = "1",
            Summary = summaryValue,
            Assignee = "admin"
        };

        await issue.SetCustomFieldAsync("Custom Text Field", "My new value");
        await issue.SetCustomFieldAsync("Custom Date Field", "2015-10-03");
        await issue.SetCustomFieldAsync("Custom Select Field", "Blue");
        await issue.SaveChangesAsync();

        var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value);
        Assert.Equal("My new value", await newIssue.GetCustomFieldAsync("Custom Text Field"));
        Assert.Equal("2015-10-03", await newIssue.GetCustomFieldAsync("Custom Date Field"));
        await newIssue.SetCustomFieldAsync("Custom Text Field", null);
        await newIssue.SetCustomFieldAsync("Custom Date Field", null);
        await newIssue.SetCustomFieldAsync("Custom Select Field", null);
        await newIssue.SaveChangesAsync();

        var updatedIssue = await jira.Issues.GetIssueAsync(issue.Key.Value);

        Assert.Null(await updatedIssue.GetCustomFieldAsync("Custom Text Field"));
        Assert.Null(await updatedIssue.GetCustomFieldAsync("Custom Date Field"));
        Assert.Null(await updatedIssue.GetCustomFieldAsync("Custom Select Field"));
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task CreateAndUpdateIssueWithComplexCustomFields(Jira jira)
    {
        var dateTime = new DateTime(2016, 11, 11, 11, 11, 0);
        var dateTimeStr = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz");
        dateTimeStr = dateTimeStr.Remove(dateTimeStr.LastIndexOf(":"), 1);
        var summaryValue = "Test issue with lots of custom fields (Created)" + _random.Next(int.MaxValue);

        // Create issue with no custom fields set
        var issue = new Issue(jira, "TST")
        {
            Type = "1",
            Summary = summaryValue,
            Assignee = "admin"
        };

        await issue.SaveChangesAsync();

        // Retrieve the issue, set all custom fields and save the changes.
        var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value);

        await newIssue.SetCustomFieldAsync("Custom Text Field", "My new value");
        await newIssue.SetCustomFieldAsync("Custom Date Field", "2015-10-03");
        await newIssue.SetCustomFieldAsync("Custom DateTime Field", dateTimeStr);
        await newIssue.SetCustomFieldAsync("Custom User Field", "admin");
        await newIssue.SetCustomFieldAsync("Custom Select Field", "Blue");
        await newIssue.SetCustomFieldAsync("Custom Group Field", "jira-users");
        await newIssue.SetCustomFieldAsync("Custom Project Field", "TST");
        await newIssue.SetCustomFieldAsync("Custom Version Field", "1.0");
        await newIssue.SetCustomFieldAsync("Custom Radio Field", "option1");
        await newIssue.SetCustomFieldAsync("Custom Number Field", "1234");
        await newIssue.CustomFields.AddArrayAsync("Custom Labels Field", "label1", "label2");
        await newIssue.CustomFields.AddArrayAsync("Custom Multi Group Field", "jira-developers", "jira-users");
        await newIssue.CustomFields.AddArrayAsync("Custom Multi Select Field", "option1", "option2");
        await newIssue.CustomFields.AddArrayAsync("Custom Multi User Field", "admin", "test");
        await newIssue.CustomFields.AddArrayAsync("Custom Checkboxes Field", "option1", "option2");
        await newIssue.CustomFields.AddArrayAsync("Custom Multi Version Field", "2.0", "3.0");
        await newIssue.CustomFields.AddArrayAsync("Custom Cascading Select Field", "Option2", "Option2.2");

        await newIssue.SaveChangesAsync();

        // Retrieve the issue again and verify fields
        var updatedIssue = await jira.Issues.GetIssueAsync(issue.Key.Value);

        Assert.Equal("My new value", await updatedIssue.GetCustomFieldAsync("Custom Text Field"));
        Assert.Equal("2015-10-03", await updatedIssue.GetCustomFieldAsync("Custom Date Field"));
        Assert.Equal("admin", await updatedIssue.GetCustomFieldAsync("Custom User Field"));
        Assert.Equal("Blue", await updatedIssue.GetCustomFieldAsync("Custom Select Field"));
        Assert.Equal("jira-users", await updatedIssue.GetCustomFieldAsync("Custom Group Field"));
        Assert.Equal("TST", await updatedIssue.GetCustomFieldAsync("Custom Project Field"));
        Assert.Equal("1.0", await updatedIssue.GetCustomFieldAsync("Custom Version Field"));
        Assert.Equal("option1", await updatedIssue.GetCustomFieldAsync("Custom Radio Field"));
        Assert.Equal("1234", await updatedIssue.GetCustomFieldAsync("Custom Number Field"));

        var serverDate = DateTime.Parse((await updatedIssue.GetCustomFieldAsync("Custom DateTime Field")).Value);
        Assert.Equal(dateTime, serverDate);

        Assert.Equal(new string[2] { "label1", "label2" }, (await updatedIssue.CustomFields.GetCustomFieldAsync("Custom Labels Field")).Values);
        Assert.Equal(new string[2] { "jira-developers", "jira-users" }, (await updatedIssue.CustomFields.GetCustomFieldAsync("Custom Multi Group Field")).Values);
        Assert.Equal(new string[2] { "option1", "option2" }, (await updatedIssue.CustomFields.GetCustomFieldAsync("Custom Multi Select Field")).Values);
        Assert.Equal(new string[2] { "admin", "test" }, (await updatedIssue.CustomFields.GetCustomFieldAsync("Custom Multi User Field")).Values);
        Assert.Equal(new string[2] { "option1", "option2" }, (await updatedIssue.CustomFields.GetCustomFieldAsync("Custom Checkboxes Field")).Values);
        Assert.Equal(new string[2] { "2.0", "3.0" }, (await updatedIssue.CustomFields.GetCustomFieldAsync("Custom Multi Version Field")).Values);

        var cascadingSelect = await updatedIssue.CustomFields.GetCascadingSelectFieldAsync("Custom Cascading Select Field");
        Assert.Equal("Option2", cascadingSelect.ParentOption);
        Assert.Equal("Option2.2", cascadingSelect.ChildOption);
        Assert.Equal("Custom Cascading Select Field", cascadingSelect.Name);

        // Update custom fields again and save
        await updatedIssue.SetCustomFieldAsync("Custom Text Field", "My newest value");
        await updatedIssue.SetCustomFieldAsync("Custom Date Field", "2019-10-03");
        await updatedIssue.SetCustomFieldAsync("Custom Number Field", "9999");
        (await updatedIssue.CustomFields.GetCustomFieldAsync("Custom Labels Field")).Values = new string[] { "label3" };
        await updatedIssue.SaveChangesAsync();

        // Retrieve the issue one last time and verify custom fields.
        var updatedIssue2 = await jira.Issues.GetIssueAsync(issue.Key.Value);
        Assert.Equal("My newest value", await updatedIssue.GetCustomFieldAsync("Custom Text Field"));
        Assert.Equal("2019-10-03", await updatedIssue.GetCustomFieldAsync("Custom Date Field"));
        Assert.Equal("9999", await updatedIssue2.GetCustomFieldAsync("Custom Number Field"));
        Assert.Equal(new string[1] { "label3" }, (await updatedIssue.CustomFields.GetCustomFieldAsync("Custom Labels Field")).Values);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task CreateAndQuerySprintName(Jira jira)
    {
        var issue = new Issue(jira, "SCRUM")
        {
            Type = "Bug",
            Summary = "Test issue with sprint" + _random.Next(int.MaxValue),
            Assignee = "admin"
        };
        // Set the sprint by id
        await issue.SetCustomFieldAsync("Sprint", "1");
        await issue.SaveChangesAsync();

        // Get the sprint by name
        var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value);
        Assert.Equal("Sprint 1", await newIssue.GetCustomFieldAsync("Sprint"));
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task UpdateAndQuerySprintName(Jira jira)
    {
        var issue = new Issue(jira, "SCRUM")
        {
            Type = "Bug",
            Summary = "Test issue with sprint" + _random.Next(int.MaxValue),
            Assignee = "admin"
        };
        await issue.SaveChangesAsync();
        Assert.Null(await issue.GetCustomFieldAsync("Sprint"));

        // Set the sprint by id
        await issue.SetCustomFieldAsync("Sprint", "1");
        await issue.SaveChangesAsync();

        // Get the sprint by name
        var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value);
        Assert.Equal("Sprint 1", await newIssue.GetCustomFieldAsync("Sprint"));
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task CanUpdateIssueWithoutModifyingCustomFields(Jira jira)
    {
        var issue = new Issue(jira, "SCRUM")
        {
            Type = "Bug",
            Summary = "Test issue with sprint" + _random.Next(int.MaxValue),
            Assignee = "admin"
        };
        await issue.SetCustomFieldAsync("Sprint", "1");
        await issue.SaveChangesAsync();
        Assert.Equal("Sprint 1", await issue.GetCustomFieldAsync("Sprint"));

        issue.Summary += " (Updated)";
        await issue.SaveChangesAsync();
        Assert.Equal("Sprint 1", await issue.GetCustomFieldAsync("Sprint"));
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task ThrowsErrorWhenSettingSprintByName(Jira jira)
    {
        var issue = new Issue(jira, "SCRUM")
        {
            Type = "Bug",
            Summary = "Test issue with sprint" + _random.Next(int.MaxValue),
            Assignee = "admin"
        };

        // Set the sprint by name
        await issue.SetCustomFieldAsync("Sprint", "Sprint 1");

        try
        {
            await issue.SaveChangesAsync();
            throw new Exception("Method did not throw exception");
        }
        catch (AggregateException ex)
        {
            Assert.Contains("Number value expected as the Sprint id", ex.Flatten().InnerException.Message);
        }
    }

    public class IssueFieldMetadataCustomFieldOption
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }
    }
}
