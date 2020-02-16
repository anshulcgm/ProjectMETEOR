using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class ProjectList
{
        public List<Project> projects = new List<Project>();

        public void SwitchProject(Enemy e, Project p)
        {
            e.currProject.enemiesOnProject.Remove(e);
            Project projectToSwitchTo = Contains(p);
            if (projectToSwitchTo == null)
            {
                return;
            }
            projectToSwitchTo.enemiesOnProject.Add(e);
            e.currProject = projectToSwitchTo;
        }

        //remove a project
        public void Remove(Project p)
        {
            Project projectToRemove = null;
            foreach(Project pr in projects)
            {
                if(pr.Equals(p))
                {
                    projectToRemove = pr;
                }
            }
            projects.Remove(projectToRemove);
            projectToRemove.projectPriority = -1;
            projects[projects.Count - 1].enemiesOnProject.AddRange(projectToRemove.enemiesOnProject);
            
            foreach(Enemy e in projectToRemove.enemiesOnProject)
            {
                e.currProject = projects[projects.Count - 1];
            }
        }

        //add a project
        public void Add(Project p)
        {
            int currProj = 0;
            while (currProj < projects.Count && p.projectPriority < projects[currProj].projectPriority)
            {
                currProj++;
            }
            projects.Insert(currProj, p);
        }

        
        public bool canTake(Project taker, Project giver)
        {
            
            double takerHeur = Math.Abs(taker.enemiesOnProject.Count - taker.optimalNumWorkers) + Math.Abs((double)taker.projectPriority / taker.optimalNumWorkers);
            double giverHeur = Math.Abs(giver.enemiesOnProject.Count - giver.optimalNumWorkers) + Math.Abs((double)giver.projectPriority / giver.optimalNumWorkers);
            giverHeur *= 2 * giver.projectPriority;
            takerHeur *= taker.projectPriority;
            return takerHeur > giverHeur;
        }

        public Project Contains(Project match)
        {
            foreach(Project p in projects)
            {
                if(p.Equals(match))
                {
                    return p;
                }
            }
            return null;
        }

        public List<Project> getProjectsWithAction (EnemyAction a)
        {
            List<Project> proj = new List<Project>();
            foreach(Project p in projects)
            {
                if(p.action == a)
                {
                    proj.Add(p);
                }
            }
            return proj;
        }

        //gets the extra enemies
        public List<Enemy> getExtraEnemies()
        {
            List<Enemy> extras = new List<Enemy>();
            for(int i = projects.Count - 1; i >= 0; i--)
            {
                for(int i1 = 0; i1 < projects[i].enemiesOnProject.Count - projects[i].optimalNumWorkers; i1++)
                {
                    extras.Add(projects[i].enemiesOnProject[i1]);
                }
            }
            return extras;
        }
}

