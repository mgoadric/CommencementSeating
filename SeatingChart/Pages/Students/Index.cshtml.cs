using SeatingChart.Data;
using SeatingChart.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using SeatingChart;
using System.ComponentModel;

namespace SeatingChart.Pages.Students
{
    public class IndexModel : PageModel
    {
        private readonly ChartContext _context;
        private readonly IConfiguration Configuration;

        public IndexModel(ChartContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }
        public int ChartNum {get;set;}
        public string NameSort { get; set; }

        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        //  public PaginatedList<Student> Students { get; set; } 

        public List<Student> Students { get; set; }
        public String [] DisplayNames { get; set; }

        public int numCols { get; set;}

        public async Task OnGetAsync(int chartNum, string sortOrder,
            string currentFilter, string searchString, int? pageIndex)
        {
            ChartNum = chartNum;
            Console.WriteLine("ccc");
            Console.WriteLine(chartNum);
            CurrentSort = sortOrder;
            NameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            IQueryable<Student> studentsIQ = from s in _context.Students
                                             select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                studentsIQ = studentsIQ.Where(s => s.LastName.Contains(searchString)
                                       || s.LastName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    studentsIQ = studentsIQ.OrderByDescending(s => s.LastName);
                    break;
    
                default:
                    studentsIQ = studentsIQ.OrderBy(s => s.LastName + " " + s.FirstName + " " + s.MiddleName); 
            
                    break;
            } 
        
            var conf = from c in _context.Configurations.Where(c => c.ID == ChartNum) select c;
            numCols = 2;
            if(await conf.AnyAsync()) {
                numCols = (await conf.FirstAsync()).NumberofColumns;
            }

            // var pageSize = Configuration.GetValue("PageSize", 4);
            // Students = await PaginatedList<Student>.CreateAsync(
                // studentsIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            Students = studentsIQ.ToList();
            DisplayNames = getDisplayNames(Students);
        }

        private String [] getDisplayNames (List<Student> students){
            Student [] studs = students.ToArray();
            Dictionary<String, List<int>> nameDic = new Dictionary<string, List<int>>();
            // String [] displayNames = new string [studs.Length];
            String [] displayNames = (from s in studs select s.LastName).ToArray();
            for (int i = 0; i < studs.Length; i++){
                if(!nameDic.ContainsKey(studs[i].LastName)){
                    nameDic.Add(studs[i].LastName, new List<int>());
                }
                nameDic[studs[i].LastName].Add(i);
            }
            HashSet<int> c = new HashSet<int>();
            Console.WriteLine(studs.Length == displayNames.Length);
            for (int i = 0; i < studs.Length; i++){
                Console.WriteLine(studs[i].LastName +" , "+displayNames[i]);
                if(!c.Contains(i)){
                    if(nameDic[displayNames[i]].Count > 1){
                        DisplayHelper(new Dictionary<String, List<int>>{{displayNames[i] , nameDic[displayNames[i]]}}, displayNames, studs, 0);
                        foreach(int j in nameDic[studs[i].LastName]){
                            c.Add(j);
                        }
                    }
                }
            }
            return displayNames;
        }
        private void DisplayHelper (Dictionary<String, List<int>> dispDic, String[] displayNames, Student [] studs, int swch){
            Dictionary<String, List<int>> newDispDic = new Dictionary<string, List<int>>();
            foreach(String s in dispDic.Keys){
                foreach(int i in dispDic[s]){
                    switch(swch){
                        case 0:
                            Console.WriteLine(0);
                            displayNames[i] = $"{studs[i].FirstName.Substring(0,1)} {displayNames[i]}";
                            break;
                        case 1:
                            Console.WriteLine(1);
                            if(studs[i].MiddleName != null){
                                displayNames[i] = $"{studs[i].FirstName.Substring(0,1)}.{studs[i].MiddleName.Substring(0,1)}. {studs[i].LastName}";
                            }
                            break;
                        case 2:
                            if(studs[i].MiddleName != null){
                                displayNames[i] = $"{studs[i].FirstName} {studs[i].MiddleName.Substring(0,1)} {studs[i].LastName}";
                            }else{
                                displayNames[i] = $"{studs[i].FirstName} {studs[i].LastName}";
                            }
                            break;
                        case 3:
                            if(studs[i].MiddleName != null){
                                displayNames[i] = $"{studs[i].FirstName} {studs[i].MiddleName} {studs[i].LastName}";
                            }
                            break;
                        default:
                            Console.WriteLine("Return");
                            return;
                    }
                    if(!newDispDic.ContainsKey(displayNames[i])){
                        newDispDic.Add(displayNames[i], new List<int>());
                    }
                    newDispDic[displayNames[i]].Add(i);
                }
            }
            List<String> toRemove = new List<String>();
            foreach(String s in newDispDic.Keys){
                Console.WriteLine("ToRemove: " + newDispDic[s].Count);
                if(!(newDispDic[s].Count > 1)){
                    toRemove.Add(s);
                }
            }
            foreach(String s in toRemove){
                newDispDic.Remove(s);
            }
            if(newDispDic.Keys.Count > 0){
                DisplayHelper(newDispDic, displayNames, studs, swch + 1);
            }
        }
    }
}