using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Epi;
using Epi.Core;
using Epi.Fields;

namespace EpiDashboard.Rules
{   

    /// <summary>
    /// A class designed to assign data to another variable using a simple dashboard function
    /// </summary>
    public class Rule_SimpleAssign : DataAssignmentRule
    {
        #region Private Members
        private SimpleAssignType assignmentType;
        private List<string> assignmentParameters;
        private List<string> resourceList;
        private bool isDefective = false;        
        #endregion // Private Members

        #region Constructors 
        /// <summary>
        /// Default constructor
        /// </summary>
        public Rule_SimpleAssign()
        {
            this.destinationColumnType = "System.Decimal";
            this.assignmentParameters = new List<string>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Rule_SimpleAssign(DashboardHelper dashboardHelper)
        {
            this.dashboardHelper = dashboardHelper;
            this.destinationColumnType = "System.Decimal";
            this.assignmentParameters = new List<string>();            
        }

        /// <summary>
        /// Constructor for simple assignment
        /// </summary>
        public Rule_SimpleAssign(DashboardHelper dashboardHelper, string friendlyRule, string destinationColumnName, SimpleAssignType assignmentType, List<string> assignmentParameters)
        {
            this.friendlyRule = friendlyRule;
            this.destinationColumnName = destinationColumnName;            
            this.DashboardHelper = dashboardHelper;
            this.variableType = DashboardVariableType.Numeric;
            this.assignmentType = assignmentType;
            this.destinationColumnType = GetDestinationColumnType(this.assignmentType);
            this.assignmentParameters = assignmentParameters;
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Whether or not the rule is defective in some way. If set to true, the rule will not be processed.
        /// </summary>
        public bool IsDefective
        {
            get
            {
                return this.isDefective;
            }
            set
            {
                this.isDefective = value;
            }
        }
        #endregion // Public Properties

        #region Public Methods

        /// <summary>
        /// Gets a list of field names that this rule cannot be run without
        /// </summary>
        /// <returns>List of strings</returns>
        public override List<string> GetDependencies()
        {
            List<string> dependencies = new List<string>();

            dependencies.Add(DestinationColumnName);

            if (AssignmentType.Equals(SimpleAssignType.YearsElapsed) || AssignmentType.Equals(SimpleAssignType.MonthsElapsed) || AssignmentType.Equals(SimpleAssignType.DaysElapsed) || AssignmentType.Equals(SimpleAssignType.HoursElapsed) || AssignmentType.Equals(SimpleAssignType.MinutesElapsed))
            {
                dependencies.Add(AssignmentParameters[0]);
                dependencies.Add(AssignmentParameters[1]);
            }

            if (
                AssignmentType.Equals(SimpleAssignType.TextToNumber) ||
                AssignmentType.Equals(SimpleAssignType.FindText) ||
                AssignmentType.Equals(SimpleAssignType.StringLength) ||
                AssignmentType.Equals(SimpleAssignType.Round) ||
                AssignmentType.Equals(SimpleAssignType.Uppercase) ||
                AssignmentType.Equals(SimpleAssignType.Lowercase) ||
                AssignmentType.Equals(SimpleAssignType.DetermineNonExistantListValues)
                )
            {
                dependencies.Add(AssignmentParameters[0]);
            }

            if (AssignmentType.Equals(SimpleAssignType.Substring))
            {
                dependencies.Add(AssignmentParameters[0]);

                if (dashboardHelper.TableColumnNames.ContainsKey(AssignmentParameters[1]))
                {
                    dependencies.Add(AssignmentParameters[1]);
                }

                if (dashboardHelper.TableColumnNames.ContainsKey(AssignmentParameters[2]))
                {
                    dependencies.Add(AssignmentParameters[2]);
                }
            }

            if (AssignmentType.Equals(SimpleAssignType.AddDays))
            {
                dependencies.Add(AssignmentParameters[0]);

                if (dashboardHelper.TableColumnNames.ContainsKey(AssignmentParameters[1]))
                {
                    dependencies.Add(AssignmentParameters[1]);
                }
            }

            if (AssignmentType.Equals(SimpleAssignType.CountCheckedCheckboxesInGroup) || 
                AssignmentType.Equals(SimpleAssignType.CountFieldsWithMissingInGroup) || 
                AssignmentType.Equals(SimpleAssignType.CountFieldsWithoutMissingInGroup) || 
                AssignmentType.Equals(SimpleAssignType.CountNumericFieldsBetweenValuesInGroup) || 
                AssignmentType.Equals(SimpleAssignType.CountNumericFieldsOutsideValuesInGroup) || 
                AssignmentType.Equals(SimpleAssignType.CountYesMarkedYesNoFieldsInGroup) || 
                AssignmentType.Equals(SimpleAssignType.DetermineCheckboxesCheckedInGroup) || 
                AssignmentType.Equals(SimpleAssignType.DetermineFieldsWithMissingInGroup) || 
                AssignmentType.Equals(SimpleAssignType.DetermineYesMarkedYesNoFieldsInGroup) || 
                AssignmentType.Equals(SimpleAssignType.FindMaxNumericFieldsInGroup) || 
                AssignmentType.Equals(SimpleAssignType.FindMeanNumericFieldsInGroup) || 
                AssignmentType.Equals(SimpleAssignType.FindMinNumericFieldsInGroup) || 
                AssignmentType.Equals(SimpleAssignType.FindSumNumericFieldsInGroup)
                )
            {
                dependencies.Add(AssignmentParameters[0]);
            }

            return dependencies;
        }

        public override void SetupRule(DataTable table)
        {
            string destinationColumnType = this.DestinationColumnType;
            DataColumn dc = new DataColumn(destinationColumnName);

            switch (destinationColumnType)
            {
                case "System.Boolean":
                    dc = new DataColumn(this.DestinationColumnName, typeof(bool));
                    break;
                case "System.Byte":
                case "System.SByte":
                    dc = new DataColumn(this.DestinationColumnName, typeof(byte));
                    break;
                case "System.Single":
                case "System.Double":
                    dc = new DataColumn(this.DestinationColumnName, typeof(double));
                    break;
                case "System.Decimal":
                    dc = new DataColumn(this.DestinationColumnName, typeof(decimal));
                    break;
                case "System.DateTime":
                    dc = new DataColumn(this.DestinationColumnName, typeof(DateTime));
                    break;
                case "System.String":
                default:
                    dc = new DataColumn(this.DestinationColumnName, typeof(string));
                    break;
            }

            if (!table.Columns.Contains(dc.ColumnName))
            {
                try
                {
                    table.Columns.Add(dc);
                }
                catch (ArgumentException)
                {
                    dc = new DataColumn(DestinationColumnName);
                    table.Columns.Add(dc);
                }
            }
            else
            {
                foreach (DataRow row in table.Rows)
                {
                    row[dc.ColumnName] = DBNull.Value;
                }
            }

            if (AssignmentType.Equals(SimpleAssignType.DetermineNonExistantListValues) && assignmentParameters.Count == 1)
            {
                resourceList = new List<string>();
                string sourceColumnName = assignmentParameters[0];

                Field field = dashboardHelper.GetAssociatedField(sourceColumnName);

                if (field != null && field is TableBasedDropDownField)
                {
                    DataTable dataTable = ((TableBasedDropDownField)field).GetSourceData();
                    //Dictionary<string, string> fieldValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    if (field is DDLFieldOfLegalValues)
                    {
                        foreach (System.Data.DataRow row in dataTable.Rows)
                        {
                            string codeColumnName = ((TableBasedDropDownField)field).CodeColumnName.Trim();
                            if (!string.IsNullOrEmpty(codeColumnName))
                            {
                                string Key = row[codeColumnName].ToString();
                                //if (!fieldValues.ContainsKey(Key))
                                if (!resourceList.Contains(Key))
                                {
                                    //fieldValues.Add(Key, Key);
                                    resourceList.Add(Key);
                                }
                            }
                        }
                    }
                    else if (field is DDLFieldOfCodes)
                    {
                        foreach (System.Data.DataRow row in dataTable.Rows)
                        {
                            string codeColumnName = ((DDLFieldOfCodes)field).TextColumnName.Trim();
                            if (!string.IsNullOrEmpty(codeColumnName))
                            {
                                string Key = row[codeColumnName].ToString();
                                //if (!fieldValues.ContainsKey(Key))
                                if (!resourceList.Contains(Key))
                                {
                                    //fieldValues.Add(Key, Key);
                                    resourceList.Add(Key);
                                }
                            }
                        }
                    }
                    else if (field is DDLFieldOfCommentLegal)
                    {
                        foreach (System.Data.DataRow row in dataTable.Rows)
                        {
                            string codeColumnName = ((TableBasedDropDownField)field).TextColumnName.Trim();
                            if (!string.IsNullOrEmpty(codeColumnName))
                            {
                                string Key = row[codeColumnName].ToString();
                                int dash = Key.IndexOf('-');
                                Key = Key.Substring(0, dash);
                                //if (!fieldValues.ContainsKey(Key))
                                if(!resourceList.Contains(Key))
                                {
                                    //fieldValues.Add(Key, Key);
                                    resourceList.Add(Key);
                                }
                            }
                        }
                    }
                }
            }
            else if (AssignmentType == SimpleAssignType.Substring)
            {                
                int start = 0;
                if (!dashboardHelper.TableColumnNames.ContainsKey(AssignmentParameters[1]))
                {                 
                    start = int.Parse(AssignmentParameters[1]);
                    --start;

                    if (start < 0)
                    {
                        this.IsDefective = true;
                        throw new DefectiveRuleException(DefectiveRuleException.RuleIssue.SubstringInvalidStartPosition, "The starting position for the substring-based simple assign variable " + this.DestinationColumnName + " is invalid. The start position is " + start.ToString() + ".");                        
                    }
                }
            }
        }
        #endregion //Public Methods

        #region Public Properties
        /// <summary>
        /// Gets the rule's expression
        /// </summary>
        public SimpleAssignType AssignmentType
        {
            get
            {
                return this.assignmentType;
            }
        }

        /// <summary>
        /// Gets the rule's assignment parameters
        /// </summary>
        public List<string> AssignmentParameters
        {
            get
            {
                return this.assignmentParameters;
            }
        }
        #endregion // Public Properties

        #region Public Methods
        /// <summary>
        /// Converts the value of the current EpiDashboard.Rule_ExpressionAssign object to its equivalent string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.FriendlyRule;
        }
        #endregion // Public Methods

        #region Protected Methods
        /// <summary>
        /// Gets the appropriate column type to create given the type of assignment being carried out
        /// </summary>
        /// <param name="sAssignmentType">The simple assign type</param>
        /// <returns>A string representing the type of a .NET DataColumn</returns>
        protected string GetDestinationColumnType(SimpleAssignType sAssignmentType)
        {
            switch (sAssignmentType)
            {
                case SimpleAssignType.DaysElapsed:
                case SimpleAssignType.HoursElapsed:
                case SimpleAssignType.MinutesElapsed:
                case SimpleAssignType.MonthsElapsed:
                case SimpleAssignType.YearsElapsed:
                case SimpleAssignType.TextToNumber:
                case SimpleAssignType.StringLength:
                case SimpleAssignType.FindText:
                case SimpleAssignType.Round:
                case SimpleAssignType.CountCheckedCheckboxesInGroup:
                case SimpleAssignType.CountYesMarkedYesNoFieldsInGroup:
                case SimpleAssignType.CountNumericFieldsBetweenValuesInGroup:
                case SimpleAssignType.CountNumericFieldsOutsideValuesInGroup:
                case SimpleAssignType.FindSumNumericFieldsInGroup:
                case SimpleAssignType.FindMeanNumericFieldsInGroup:
                case SimpleAssignType.FindMaxNumericFieldsInGroup:
                case SimpleAssignType.FindMinNumericFieldsInGroup:
                case SimpleAssignType.CountFieldsWithMissingInGroup:
                case SimpleAssignType.CountFieldsWithoutMissingInGroup:
                    return "System.Decimal";
                case SimpleAssignType.Substring:
                case SimpleAssignType.Uppercase:
                case SimpleAssignType.Lowercase:
                case SimpleAssignType.NumberToText:
                    return "System.String";
                case SimpleAssignType.TextToDate:
                case SimpleAssignType.AddDays:
                case SimpleAssignType.StripDate:
                case SimpleAssignType.NumberToDate:
                    return "System.DateTime";
                case SimpleAssignType.DetermineNonExistantListValues:
                case SimpleAssignType.DetermineCheckboxesCheckedInGroup:
                case SimpleAssignType.DetermineYesMarkedYesNoFieldsInGroup:
                case SimpleAssignType.DetermineFieldsWithMissingInGroup:
                    return "System.Boolean";
            }

            return "System.Decimal";
        }

        /// <summary>
        /// Gets a column type appropriate for a .NET data table based off of the dashboard variable type selected by the user
        /// </summary>
        /// <param name="dashboardVariableType">The type of variable that is storing the recoded values</param>
        /// <returns>A string representing the type of a .NET DataColumn</returns>
        protected string GetDestinationColumnType(DashboardVariableType dashboardVariableType)
        {
            switch (dashboardVariableType)
            {
                case DashboardVariableType.Numeric:
                    return "System.Single";
                case DashboardVariableType.Text:
                    return "System.String";
                case DashboardVariableType.YesNo:
                    return "System.Byte";
                case DashboardVariableType.Date:
                    return "System.DateTime";
                case DashboardVariableType.None:
                    throw new ApplicationException(SharedStrings.DASHBOARD_ERROR_INVALID_COLUMN_TYPE);
                default:
                    return "System.String";
            }
        }
        #endregion // Protected Methods

        #region IDashboardRule Members
        /// <summary>
        /// Generates an Xml element for this rule
        /// </summary>
        /// <param name="doc">The parent Xml document</param>
        /// <returns>XmlNode representing this rule</returns>
        public override System.Xml.XmlNode Serialize(System.Xml.XmlDocument doc)
        {
            string xmlString =
            "<friendlyRule>" + friendlyRule + "</friendlyRule>" +
            "<assignmentType>" + ((int)assignmentType).ToString() + "</assignmentType>" +
            "<destinationColumnName>" + destinationColumnName + "</destinationColumnName>" +
            "<destinationColumnType>" + destinationColumnType + "</destinationColumnType>";

            xmlString = xmlString + "<parameterList>";
            foreach (string parameter in this.AssignmentParameters)
            {
                DateTime dt;
                DateTime? dtN = null;
                if (DateTime.TryParse(parameter, out dt)) dtN = dt;

                if (dtN.HasValue)
                {
                    xmlString = xmlString + "<parameter type=\"literal\" unit=\"ticks\">" + dtN.Value.Ticks + "</parameter>";
                }
                else
                {
                    xmlString = xmlString + "<parameter type=\"fieldName\">" + parameter + "</parameter>";
                }
            }
            xmlString = xmlString + "</parameterList>";

            System.Xml.XmlElement element = doc.CreateElement("rule");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute order = doc.CreateAttribute("order");
            System.Xml.XmlAttribute type = doc.CreateAttribute("ruleType");
            
            type.Value = "EpiDashboard.Rules.Rule_SimpleAssign";
            
            element.Attributes.Append(type);

            return element;
        }

        /// <summary>
        /// Creates the rule from an Xml element
        /// </summary>
        /// <param name="element">The XmlElement from which to create the rule</param>
        public override void CreateFromXml(System.Xml.XmlElement element)
        {
            foreach (XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("friendlyRule"))
                {
                    this.friendlyRule = child.InnerText;
                }
                else if (child.Name.Equals("assignmentType"))
                {                    
                    this.assignmentType = ((SimpleAssignType)Int32.Parse(child.InnerText));
                }
                else if (child.Name.Equals("destinationColumnName"))
                {
                    this.destinationColumnName = child.InnerText;
                }
                else if (child.Name.Equals("destinationColumnType"))
                {
                    this.destinationColumnType = child.InnerText;
                }
                else if (child.Name.Equals("parameterList"))
                {
                    foreach (XmlElement parameter in child.ChildNodes)
                    {
                        if (parameter.Name.ToLowerInvariant().Equals("parameter"))
                        {
                            if (parameter.Attributes.Count > 0 && parameter.Attributes[0].Value.ToLowerInvariant().Equals("literal") && parameter.Attributes[1].Value.ToLowerInvariant().Equals("ticks"))
                            {
                                DateTime dt = new DateTime(long.Parse(parameter.InnerText));
                                AssignmentParameters.Add(dt.ToString());
                            }
                            else
                            {
                                AssignmentParameters.Add(parameter.InnerText);
                            }
                        }
                    }
                }
            }

            if (destinationColumnType.Equals("System.String"))
            {
                this.variableType = DashboardVariableType.Text;
            }
            else if(destinationColumnType.Equals("System.Single") || destinationColumnType.Equals("System.Double") || destinationColumnType.Equals("System.Decimal") || destinationColumnType.Equals("System.Int16") || destinationColumnType.Equals("System.Int32"))
            {
                this.variableType = DashboardVariableType.Numeric;
            }
            else if (destinationColumnType.Equals("System.DateTime"))
            {
                this.variableType = DashboardVariableType.Date;
            }
        }

        /// <summary>
        /// Applies the rule
        /// </summary>
        public override void ApplyRule(DataRow row)
        {
            if (IsDefective)
            {
                return;
            }

            if (AssignmentType.Equals(SimpleAssignType.YearsElapsed))
            {
                string minDateColumnName = AssignmentParameters[0];
                string maxDateColumnName = AssignmentParameters[1];

                DateTime minDate;
                DateTime? minDateN = null;
                if (DateTime.TryParse(minDateColumnName, out minDate)) minDateN = minDate;

                DateTime maxDate;
                DateTime? maxDateN = null;
                if (DateTime.TryParse(maxDateColumnName, out maxDate)) maxDateN = maxDate;

                if (minDateN == null && row[minDateColumnName] != null && row[minDateColumnName] != DBNull.Value)
                {
                    minDate = (DateTime)row[minDateColumnName];
                    minDateN = minDate;
                }

                if (maxDateN == null && row[maxDateColumnName] != null && row[maxDateColumnName] != DBNull.Value)
                {
                    maxDate = (DateTime)row[maxDateColumnName];
                    maxDateN = maxDate;
                }

                if (minDateN == null || string.IsNullOrEmpty(minDateColumnName)) 
                    row[this.DestinationColumnName] = DBNull.Value;
                else if (maxDateN == null || string.IsNullOrEmpty(maxDateColumnName))
                    row[this.DestinationColumnName] = DBNull.Value;
                else
                {
                    int years = maxDate.Year - minDate.Year;
                    if
                    (
                        maxDate.Month < minDate.Month ||
                        (maxDate.Month == minDate.Month && maxDate.Day < minDate.Day)
                    )
                    {
                        years--;
                    }

                    row[this.DestinationColumnName] = years;
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.MonthsElapsed))
            {
                string minDateColumnName = AssignmentParameters[0];
                string maxDateColumnName = AssignmentParameters[1];

                DateTime minDate;
                DateTime? minDateN = null;
                if (DateTime.TryParse(minDateColumnName, out minDate)) minDateN = minDate;

                DateTime maxDate;
                DateTime? maxDateN = null;
                if (DateTime.TryParse(maxDateColumnName, out maxDate)) maxDateN = maxDate;

                if (minDateN == null && row[minDateColumnName] != null && row[minDateColumnName] != DBNull.Value)
                {
                    minDate = (DateTime)row[minDateColumnName];
                    minDateN = minDate;
                }

                if (maxDateN == null && row[maxDateColumnName] != null && row[maxDateColumnName] != DBNull.Value)
                {
                    maxDate = (DateTime)row[maxDateColumnName];
                    maxDateN = maxDate;
                }

                if (minDateN == null || string.IsNullOrEmpty(minDateColumnName))
                    row[this.DestinationColumnName] = DBNull.Value;
                else if (maxDateN == null || string.IsNullOrEmpty(maxDateColumnName))
                    row[this.DestinationColumnName] = DBNull.Value;
                else
                {
                    int monthsApart = 12 * (maxDate.Year - minDate.Year) + maxDate.Month - minDate.Month;

                    if (maxDate.Day < minDate.Day)
                    {
                        monthsApart--;
                    }

                    row[this.DestinationColumnName] = monthsApart;
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.DaysElapsed))
            {
                string minDateColumnName = AssignmentParameters[0];
                string maxDateColumnName = AssignmentParameters[1];

                DateTime minDate;
                DateTime? minDateN = null;
                if (DateTime.TryParse(minDateColumnName, out minDate)) minDateN = minDate;

                DateTime maxDate;
                DateTime? maxDateN = null;
                if (DateTime.TryParse(maxDateColumnName, out maxDate)) maxDateN = maxDate;

                if (minDateN == null && row[minDateColumnName] != null && row[minDateColumnName] != DBNull.Value)
                {
                    minDate = (DateTime)row[minDateColumnName];
                    minDateN = minDate;
                }

                if (maxDateN == null && row[maxDateColumnName] != null && row[maxDateColumnName] != DBNull.Value)
                {
                    maxDate = (DateTime)row[maxDateColumnName];
                    maxDateN = maxDate;
                }

                if (minDateN == null || string.IsNullOrEmpty(minDateColumnName))
                    row[this.DestinationColumnName] = DBNull.Value;
                else if (maxDateN == null || string.IsNullOrEmpty(maxDateColumnName))
                    row[this.DestinationColumnName] = DBNull.Value;
                else
                {
                    TimeSpan timeSpan = maxDate.Subtract(minDate);
                    double days = Math.Round(timeSpan.TotalDays);
                    row[this.DestinationColumnName] = days;
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.HoursElapsed))
            {
                string minDateColumnName = AssignmentParameters[0];
                string maxDateColumnName = AssignmentParameters[1];

                DateTime minDate;
                DateTime? minDateN = null;
                if (DateTime.TryParse(minDateColumnName, out minDate)) minDateN = minDate;

                DateTime maxDate;
                DateTime? maxDateN = null;
                if (DateTime.TryParse(maxDateColumnName, out maxDate)) maxDateN = maxDate;

                if (minDateN == null && row[minDateColumnName] != null && row[minDateColumnName] != DBNull.Value)
                {
                    minDate = (DateTime)row[minDateColumnName];
                    minDateN = minDate;
                }

                if (maxDateN == null && row[maxDateColumnName] != null && row[maxDateColumnName] != DBNull.Value)
                {
                    maxDate = (DateTime)row[maxDateColumnName];
                    maxDateN = maxDate;
                }

                if (minDateN == null || string.IsNullOrEmpty(minDateColumnName))
                    row[this.DestinationColumnName] = DBNull.Value;
                else if (maxDateN == null || string.IsNullOrEmpty(maxDateColumnName))
                    row[this.DestinationColumnName] = DBNull.Value;
                else
                {
                    TimeSpan timeSpan = maxDate.Subtract(minDate);
                    double hours = Math.Round(timeSpan.TotalHours);
                    row[this.DestinationColumnName] = hours;
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.MinutesElapsed))
            {
                string minDateColumnName = AssignmentParameters[0];
                string maxDateColumnName = AssignmentParameters[1];

                DateTime minDate;
                DateTime? minDateN = null;
                if (DateTime.TryParse(minDateColumnName, out minDate)) minDateN = minDate;

                DateTime maxDate;
                DateTime? maxDateN = null;
                if (DateTime.TryParse(maxDateColumnName, out maxDate)) maxDateN = maxDate;

                if (minDateN == null && row[minDateColumnName] != null && row[minDateColumnName] != DBNull.Value)
                {
                    minDate = (DateTime)row[minDateColumnName];
                    minDateN = minDate;
                }

                if (maxDateN == null && row[maxDateColumnName] != null && row[maxDateColumnName] != DBNull.Value)
                {
                    maxDate = (DateTime)row[maxDateColumnName];
                    maxDateN = maxDate;
                }

                if (minDateN == null || string.IsNullOrEmpty(minDateColumnName))
                    row[this.DestinationColumnName] = DBNull.Value;
                else if (maxDateN == null || string.IsNullOrEmpty(maxDateColumnName))
                    row[this.DestinationColumnName] = DBNull.Value;
                else
                {
                    TimeSpan timeSpan = maxDate.Subtract(minDate);
                    double hours = Math.Round(timeSpan.TotalMinutes);
                    row[this.DestinationColumnName] = hours;
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.TextToNumber))
            {
                string textColumnName = AssignmentParameters[0];
                string value = row[textColumnName].ToString().Trim();

                if (row[textColumnName] == null || string.IsNullOrEmpty(value))
                {
                    row[this.DestinationColumnName] = DBNull.Value;
                }
                else
                {
                    double result;
                    bool success = double.TryParse(value, out result);

                    if (success)
                    {
                        row[this.DestinationColumnName] = result;
                    }
                    else
                    {
                        row[this.DestinationColumnName] = DBNull.Value;
                    }
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.TextToDate))
            {
                string textColumnName = AssignmentParameters[0];
                string value = row[textColumnName].ToString().Trim();
                if (row[textColumnName] == null || string.IsNullOrEmpty(value))
                {
                    row[this.DestinationColumnName] = DBNull.Value;
                }
                else
                {
                    try
                    {
                        DateTime? dateField = DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                        row[this.DestinationColumnName] = dateField;
                    }
                    catch (Exception)
                    {
                        row[this.DestinationColumnName] = DBNull.Value;
                    }
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.NumberToDate))
            {
                int year;
                int month;
                int day;

                string columnNameDay = AssignmentParameters[0];
                string columnNameMonth = AssignmentParameters[1];
                string columnNameYear = AssignmentParameters[2];

                string valueDay = row[columnNameDay].ToString().Trim();
                string valueMonth = row[columnNameMonth].ToString().Trim();
                string valueYear = row[columnNameYear].ToString().Trim();

                if (Int32.TryParse(valueYear, out year))
                {
                    if (Int32.TryParse(valueMonth, out month))
                    {
                        if (Int32.TryParse(valueDay, out day))
                        {
                            try
                            {
                                DateTime? dateField = new DateTime(year, month, day);
                                row[this.DestinationColumnName] = dateField;
                            }
                            catch (Exception)
                            {
                                row[this.DestinationColumnName] = DBNull.Value;
                            }
                        }
                    }
                }

            }
            else if (AssignmentType.Equals(SimpleAssignType.FindText))
            {
                string textColumnName = AssignmentParameters[0];
                string searchString = AssignmentParameters[1];

                string value = row[textColumnName].ToString();
                int indexOf = value.IndexOf(searchString);

                //Commenting for EI-361
                //if (indexOf == -1)
                //{
                //    row[this.DestinationColumnName] = DBNull.Value;
                //}
                //else
                //{
                    row[this.DestinationColumnName] = indexOf;
                //}
            }
            else if (AssignmentType.Equals(SimpleAssignType.NumberToText))
            {
                string textColumnName = AssignmentParameters[0];
                row[this.DestinationColumnName] = row[textColumnName].ToString();
            }
            else if (AssignmentType.Equals(SimpleAssignType.StripDate))
            {
                string dateColumnName = AssignmentParameters[0];
                if(row[dateColumnName] != DBNull.Value && !string.IsNullOrEmpty(row[dateColumnName].ToString())) 
                {
                    DateTime dt = (DateTime)row[dateColumnName];
                    DateTime strippedDt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
                    row[this.DestinationColumnName] = strippedDt;
                }                                
            }
            else if (AssignmentType.Equals(SimpleAssignType.StringLength))
            {
                string textColumnName = AssignmentParameters[0];

                string value = row[textColumnName].ToString();
                row[this.DestinationColumnName] = value.Length;
            }
            else if (AssignmentType.Equals(SimpleAssignType.Round))
            {
                string numericColumnName = AssignmentParameters[0];
                int decimals = 0;
                int.TryParse(AssignmentParameters[1], out decimals);

                string value = row[numericColumnName].ToString().Trim();

                if (row[numericColumnName] == null || string.IsNullOrEmpty(value))
                {
                    row[this.DestinationColumnName] = DBNull.Value;
                }
                else
                {
                    decimal result;
                    bool success = decimal.TryParse(value, out result);
                    result = Math.Round(result, decimals);

                    if (success)
                    {
                        row[this.DestinationColumnName] = result;
                    }
                    else
                    {
                        row[this.DestinationColumnName] = DBNull.Value;
                    }
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.Substring))
            {
                string textColumnName = AssignmentParameters[0];
                int start = 0;
                int length = 0;

                if (dashboardHelper.TableColumnNames.ContainsKey(AssignmentParameters[1]))
                {
                    start = int.Parse(row[AssignmentParameters[1]].ToString());
                }
                else
                {
                    start = int.Parse(AssignmentParameters[1]);
                }

                if (dashboardHelper.TableColumnNames.ContainsKey(AssignmentParameters[2]))
                {
                    length = int.Parse(row[AssignmentParameters[2]].ToString());
                }
                else
                {
                    length = int.Parse(AssignmentParameters[2]);
                }

                --start;

                string value = row[textColumnName].ToString();

                if (start > row[textColumnName].ToString().Length)
                {
                    row[this.DestinationColumnName] = DBNull.Value;
                }
                else if (start + length > row[textColumnName].ToString().Length)
                {
                    int fullLength = row[textColumnName].ToString().Length;
                    int newLength = fullLength - start;
                    value = row[textColumnName].ToString().Substring(start, newLength);
                }
                else
                {
                    value = row[textColumnName].ToString().Substring(start, length);
                }

                row[this.DestinationColumnName] = value;          
            }
            else if (AssignmentType.Equals(SimpleAssignType.Uppercase))
            {
                string textColumnName = AssignmentParameters[0];
                row[this.DestinationColumnName] = row[textColumnName].ToString().ToUpperInvariant();
            }
            else if (AssignmentType.Equals(SimpleAssignType.Lowercase))
            {
                string textColumnName = AssignmentParameters[0];
                row[this.DestinationColumnName] = row[textColumnName].ToString().ToLowerInvariant();
            }

            else if (AssignmentType.Equals(SimpleAssignType.AddDays))
            {
                string dateColumnName = AssignmentParameters[0];
                string daysToAdd = AssignmentParameters[1];
                double days = -1;

                if (row[dateColumnName] == null || string.IsNullOrEmpty(row[dateColumnName].ToString()) || string.IsNullOrEmpty(daysToAdd))
                {
                    row[this.DestinationColumnName] = DBNull.Value;
                }
                else
                {
                    if (dashboardHelper.GetFieldsAsList(ColumnDataType.Numeric).Contains(daysToAdd))
                    {
                        bool success = double.TryParse(row[daysToAdd].ToString(), out days);
                        if (!success)
                        {
                            row[this.DestinationColumnName] = DBNull.Value;
                            return;
                        }
                    }
                    else
                    {
                        bool success = double.TryParse(daysToAdd, out days);
                        if (!success)
                        {
                            row[this.DestinationColumnName] = DBNull.Value;
                            return;
                        }
                    }

                    DateTime date = (DateTime)row[dateColumnName];
                    DateTime newDate = date.AddDays(days);

                    row[this.DestinationColumnName] = newDate;
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.DetermineNonExistantListValues))
            {
                string listColumnName = AssignmentParameters[0];
                string listValue = row[listColumnName].ToString();

                if (resourceList.Contains(listValue))
                {
                    row[this.DestinationColumnName] = false;
                }
                else
                {
                    row[this.DestinationColumnName] = true;
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.CountCheckedCheckboxesInGroup))
            {
                string groupFieldName = AssignmentParameters[0];
                int count = 0;
                foreach (string childFieldName in dashboardHelper.GetVariablesInGroup(groupFieldName))
                {
                    Field groupedField = dashboardHelper.GetAssociatedField(childFieldName);
                    if ((groupedField != null && groupedField is CheckBoxField) || dashboardHelper.IsColumnBoolean(childFieldName))
                    {
                        bool value = bool.Parse(row[childFieldName].ToString());
                        if (value) count++;
                    }
                }
                row[this.DestinationColumnName] = count;
            }
            else if (AssignmentType.Equals(SimpleAssignType.CountYesMarkedYesNoFieldsInGroup))
            {
                string groupFieldName = AssignmentParameters[0];
                int count = 0;
                foreach (string childFieldName in dashboardHelper.GetVariablesInGroup(groupFieldName))
                {
                    Field groupedField = dashboardHelper.GetAssociatedField(childFieldName);
                    if (groupedField != null && groupedField is YesNoField && !string.IsNullOrEmpty(row[childFieldName].ToString()))
                    {
                        int value = int.Parse(row[childFieldName].ToString());
                        if (value == 1) count++;
                    }
                }
                row[this.DestinationColumnName] = count;
            }
            else if (AssignmentType.Equals(SimpleAssignType.DetermineCheckboxesCheckedInGroup))
            {
                string groupFieldName = AssignmentParameters[0];
                string Nvalue = AssignmentParameters[1];

                int count = 0;
                int limit = -1;

                bool success = int.TryParse(Nvalue, out limit);
                if (success)
                {
                    foreach (string childFieldName in dashboardHelper.GetVariablesInGroup(groupFieldName))
                    {
                        Field groupedField = dashboardHelper.GetAssociatedField(childFieldName);
                        if ((groupedField != null && groupedField is CheckBoxField) || dashboardHelper.IsColumnBoolean(childFieldName))
                        {
                            bool value = bool.Parse(row[childFieldName].ToString());
                            if (value) count++;
                        }                        
                    }

                    if (count > limit)
                    {
                        row[this.DestinationColumnName] = true;
                    }
                    else
                    {
                        row[this.DestinationColumnName] = false;
                    }
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.DetermineYesMarkedYesNoFieldsInGroup))
            {
                string groupFieldName = AssignmentParameters[0];
                string Nvalue = AssignmentParameters[1];

                int count = 0;
                int limit = -1;

                bool success = int.TryParse(Nvalue, out limit);
                if (success)
                {
                    foreach (string childFieldName in dashboardHelper.GetVariablesInGroup(groupFieldName))
                    {
                        Field groupedField = dashboardHelper.GetAssociatedField(childFieldName);
                        if (groupedField != null && groupedField is YesNoField && !string.IsNullOrEmpty(row[childFieldName].ToString()))
                        {
                            int value = int.Parse(row[childFieldName].ToString());
                            if (value == 1) count++;
                        }
                    }

                    if (count > limit)
                    {
                        row[this.DestinationColumnName] = true;
                    }
                    else
                    {
                        row[this.DestinationColumnName] = false;
                    }
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.CountNumericFieldsBetweenValuesInGroup))
            {
                string groupFieldName = AssignmentParameters[0];
                string lowerValue = AssignmentParameters[1];
                string upperValue = AssignmentParameters[2];

                double lower = -1;
                double upper = -1;

                int count = 0;

                bool success1 = double.TryParse(lowerValue, out lower);
                bool success2 = double.TryParse(upperValue, out upper);

                if (success1 && success2)
                {
                    foreach (string childFieldName in dashboardHelper.GetVariablesInGroup(groupFieldName))
                    {
                        if (row.Table.Columns.Contains(childFieldName) && dashboardHelper.IsColumnNumeric(childFieldName) && !string.IsNullOrEmpty(row[childFieldName].ToString()))
                        {
                            Field field = dashboardHelper.GetAssociatedField(childFieldName);
                            if ((field != null && !(field is YesNoField)) || field == null)
                            {
                                double value = double.Parse(row[childFieldName].ToString());
                                if (value <= upper && value >= lower) count++;
                            }
                        }
                    }

                    row[this.DestinationColumnName] = count;                    
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.CountNumericFieldsOutsideValuesInGroup))
            {
                string groupFieldName = AssignmentParameters[0];
                string lowerValue = AssignmentParameters[1];
                string upperValue = AssignmentParameters[2];

                double lower = -1;
                double upper = -1;

                int count = 0;

                bool success1 = double.TryParse(lowerValue, out lower);
                bool success2 = double.TryParse(upperValue, out upper);

                if (success1 && success2)
                {
                    foreach (string childFieldName in dashboardHelper.GetVariablesInGroup(groupFieldName))
                    {
                        if (row.Table.Columns.Contains(childFieldName) && dashboardHelper.IsColumnNumeric(childFieldName) && !string.IsNullOrEmpty(row[childFieldName].ToString()))
                        {
                            Field field = dashboardHelper.GetAssociatedField(childFieldName);
                            if ((field != null && !(field is YesNoField)) || field == null)
                            {
                                double value = double.Parse(row[childFieldName].ToString());
                                if (value > upper || value < lower) count++;
                            }
                        }
                    }

                    row[this.DestinationColumnName] = count;
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.FindSumNumericFieldsInGroup))
            {
                string groupFieldName = AssignmentParameters[0];                

                bool includeYesNo = false;
                bool includeCommentLegal = false;

                if(AssignmentParameters[1] == dashboardHelper.Config.Settings.RepresentationOfYes) includeYesNo = true;
                if(AssignmentParameters[2] == dashboardHelper.Config.Settings.RepresentationOfYes) includeCommentLegal = true;                
                
                double sum = 0;
                bool resultUnknown = false;
                
                foreach (string childFieldName in dashboardHelper.GetVariablesInGroup(groupFieldName))
                {
                    Field field = dashboardHelper.GetAssociatedField(childFieldName);

                    if (row.Table.Columns.Contains(childFieldName) && dashboardHelper.IsColumnNumeric(childFieldName))
                    {
                        if (!(field != null && !includeYesNo && field is YesNoField))
                        {
                            if (!string.IsNullOrEmpty(row[childFieldName].ToString()))
                            {
                                double value = double.Parse(row[childFieldName].ToString());
                                sum = sum + value;
                            }
                            else
                            {
                                resultUnknown = true;
                                break;
                            }
                        }
                    }
                    else if (field != null && includeYesNo && field is YesNoField)
                    {
                        if (!string.IsNullOrEmpty(row[childFieldName].ToString()))
                        {
                            int value = int.Parse(row[childFieldName].ToString());
                            sum = sum + ((double)value);
                        }
                        else
                        {
                            resultUnknown = true;
                            break;
                        }
                    }
                    else if (field != null && includeCommentLegal && field is DDLFieldOfCommentLegal)
                    {
                        if (!string.IsNullOrEmpty(row[childFieldName].ToString()))
                        {
                            double value = 0;

                            bool success = double.TryParse(row[childFieldName].ToString(), out value);
                            if (success) sum = sum + value;
                        }
                        else
                        {
                            resultUnknown = true;
                            break;
                        }
                    } 
                }

                if (resultUnknown)
                {
                    row[this.DestinationColumnName] = DBNull.Value;
                }
                else
                {
                    row[this.DestinationColumnName] = sum;
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.FindMeanNumericFieldsInGroup))
            {
                string groupFieldName = AssignmentParameters[0];

                bool includeYesNo = false;
                bool includeCommentLegal = false;

                if (AssignmentParameters[1] == dashboardHelper.Config.Settings.RepresentationOfYes) includeYesNo = true;
                if (AssignmentParameters[2] == dashboardHelper.Config.Settings.RepresentationOfYes) includeCommentLegal = true; 

                double sum = 0;
                double numFields = 0;

                bool resultUnknown = false;
                List<string> includedFieldNames = new List<string>();

                foreach (string childFieldName in dashboardHelper.GetVariablesInGroup(groupFieldName))
                {
                    Field field = dashboardHelper.GetAssociatedField(childFieldName);

                    if (row.Table.Columns.Contains(childFieldName) && dashboardHelper.IsColumnNumeric(childFieldName))
                    {
                        if (!(field != null && !includeYesNo && field is YesNoField))
                        {
                            includedFieldNames.Add(childFieldName);                            
                        }
                    }
                    else if (field != null && includeYesNo && field is YesNoField)
                    {
                        includedFieldNames.Add(childFieldName);                        
                    }
                    else if (field != null && includeCommentLegal && field is DDLFieldOfCommentLegal)
                    {
                        includedFieldNames.Add(childFieldName);                        
                    }
                }

                numFields = includedFieldNames.Count;

                foreach (string fieldName in includedFieldNames)
                {
                    Field field = dashboardHelper.GetAssociatedField(fieldName);

                    if (row.Table.Columns.Contains(fieldName) && dashboardHelper.IsColumnNumeric(fieldName))
                    {
                        if (!(field != null && !includeYesNo && field is YesNoField))
                        {
                            if (!string.IsNullOrEmpty(row[fieldName].ToString()))
                            {
                                double value = double.Parse(row[fieldName].ToString());
                                sum = sum + value;
                            }
                            else
                            {
                                resultUnknown = true;
                                break;
                            }
                        }
                    }
                    else if (field != null && includeYesNo && field is YesNoField)
                    {
                        if (!string.IsNullOrEmpty(row[fieldName].ToString()))
                        {
                            int value = int.Parse(row[fieldName].ToString());
                            sum = sum + ((double)value);
                        }
                        else
                        {
                            resultUnknown = true;
                            break;
                        }
                    }
                    else if (field != null && includeCommentLegal && field is DDLFieldOfCommentLegal)
                    {
                        if (!string.IsNullOrEmpty(row[fieldName].ToString()))
                        {
                            double value = 0;

                            bool success = double.TryParse(row[fieldName].ToString(), out value);
                            if (success) { sum = sum + value; }
                        }
                        else
                        {
                            resultUnknown = true;
                            break;
                        }
                    }   
                }

                if (resultUnknown)
                {
                    row[this.DestinationColumnName] = DBNull.Value;
                }
                else
                {
                    row[this.DestinationColumnName] = sum / numFields;
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.FindMaxNumericFieldsInGroup))
            {
                string groupFieldName = AssignmentParameters[0];

                double maxValue = double.MinValue;
                bool noValues = true;

                foreach (string childFieldName in dashboardHelper.GetVariablesInGroup(groupFieldName))
                {
                    if (row.Table.Columns.Contains(childFieldName) && dashboardHelper.IsColumnNumeric(childFieldName) && !string.IsNullOrEmpty(row[childFieldName].ToString()))
                    {
                        Field field = dashboardHelper.GetAssociatedField(childFieldName);
                        if (!(field != null && field is YesNoField))
                        {
                            double value = double.Parse(row[childFieldName].ToString());
                            if (value > maxValue)
                            {
                                maxValue = value;
                                noValues = false;
                            }
                        }
                    }
                }                

                if (noValues)
                {
                    row[this.DestinationColumnName] = DBNull.Value;
                }
                else
                {
                    row[this.DestinationColumnName] = maxValue;
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.FindMinNumericFieldsInGroup))
            {
                string groupFieldName = AssignmentParameters[0];

                double minValue = double.MaxValue;
                bool noValues = true;

                foreach (string childFieldName in dashboardHelper.GetVariablesInGroup(groupFieldName))
                {
                    if (row.Table.Columns.Contains(childFieldName) && dashboardHelper.IsColumnNumeric(childFieldName) && !string.IsNullOrEmpty(row[childFieldName].ToString()))
                    {
                        Field field = dashboardHelper.GetAssociatedField(childFieldName);
                        if (!(field != null && field is YesNoField))
                        {
                            double value = double.Parse(row[childFieldName].ToString());
                            if (value < minValue)
                            {
                                minValue = value;
                                noValues = false;
                            }
                        }
                    }
                }

                if (noValues)
                {
                    row[this.DestinationColumnName] = DBNull.Value;
                }
                else
                {
                    row[this.DestinationColumnName] = minValue;
                }
            }
            else if (AssignmentType.Equals(SimpleAssignType.CountFieldsWithMissingInGroup))
            {
                string groupFieldName = AssignmentParameters[0];
                int missingCount = 0;

                foreach (string childFieldName in dashboardHelper.GetVariablesInGroup(groupFieldName))
                {
                    if (row.Table.Columns.Contains(childFieldName))
                    {
                        if (string.IsNullOrEmpty(row[childFieldName].ToString())) missingCount++;
                    }
                }
                
                row[this.DestinationColumnName] = missingCount;
            }
            else if (AssignmentType.Equals(SimpleAssignType.CountFieldsWithoutMissingInGroup))
            {
                string groupFieldName = AssignmentParameters[0];
                int notMissingCount = 0;

                foreach(string childFieldName in dashboardHelper.GetVariablesInGroup(groupFieldName)) 
                {
                    if(row.Table.Columns.Contains(childFieldName) && !string.IsNullOrEmpty(row[childFieldName].ToString()))
                    {
                        notMissingCount++;
                    }
                }

                row[this.DestinationColumnName] = notMissingCount;
            }
            else if (AssignmentType.Equals(SimpleAssignType.DetermineFieldsWithMissingInGroup))
            {
                string groupFieldName = AssignmentParameters[0];
                string Nvalue = AssignmentParameters[1];

                int limit = -1;

                bool success = int.TryParse(Nvalue, out limit);
                if (success)
                {
                    int missingCount = 0;

                    List<string> groupedFields = dashboardHelper.GetVariablesInGroup(groupFieldName);

                    foreach (string childFieldName in groupedFields)
                    {
                        if (string.IsNullOrEmpty(row[childFieldName].ToString())) missingCount++;
                    }
                    
                    if (missingCount > limit)
                    {
                        row[this.DestinationColumnName] = true;
                    }
                    else
                    {
                        row[this.DestinationColumnName] = false;
                    }
                }
                else
                {
                    // throw exception?
                    row[this.DestinationColumnName] = false;
                }
            }
        }
        #endregion // IDashboardRule Members
    }
}
