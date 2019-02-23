using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SuperheroApi.DbModels;

/**********************************************************************************************
   *Contains all of the http methods for the superhero api.
   *The api uses an in-memory database that does not persist.
 **********************************************************************************************/
namespace SuperheroApi.Controllers
{

    [Route("api")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        //Create and set the context of the in-memory database that the api uses. 
        //This is achieved using Entity Framework's in memory package
        private ApiContext _context;
        public ValuesController(ApiContext context)
        {
            _context = context;
        }



        /************************************************************************************
         * GET: api/values. Retuns a json array all the superheros in the database.
         ***********************************************************************************/
        [HttpGet]
        [Route("superhero")]
        public string Get()
        {
            //Query the data from the in-memory database
            var data = from d in _context.Users select d;

            //format the data as json and return it
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            return jsonString;
        }




        /***********************************************************************************************
         *GET: api/squadron/{name}/candidates. Returns all superheros that are not enemies of any superhero
          in the squadron with the given name *The squadron name is determined when the squadron is created 
         * Here is an example of what a request url might look like:
                localhost/api/squadron/squadrenNameICreated/candidates
        **********************************************************************************************/
        [Route("squadron/{name}/candidates")]
        public string Get2(string name)
        {
            //List to hold all valid candidates
            List<string> candidates = new List<string>();

            //Query the names of all the superheros in the squad
            var squadMembers = (from d in _context.Squad
                                where d.name == name
                                select d.Squad).SingleOrDefault();

            //Return error if invalid squadron entered(ie nothing in the squad)
            if (squadMembers == null) return "Error: squadron with that name not found";

            //Get the same names byt as strings
            var squadMembersAsString = (from d in squadMembers
                                        select d.Alias).ToList();
           

            //query a list of all the enemies of the squad members using the list of squad members. Build this into a list
            List<string> squadMemberEnemies = new List<string>();
            foreach(var mem in squadMembers)
            {
                var enemey = (from d in _context.Users
                              where d.Alias == mem.Alias
                              select d.Archenemy).SingleOrDefault();

                if (enemey != null) squadMemberEnemies.Add(enemey);
            }

            //get a list of all superheros
            var superheroes = (from d in _context.Users
                              select d.Alias).ToList();

            //For every super hero, check if they are a villain of any member of the squad
            foreach(var sup in superheroes)
            {
                bool isValidCandidate = true;
                foreach (var enemy in squadMemberEnemies)
                {
                    if (enemy == sup) isValidCandidate = false;
                }
                if (isValidCandidate && !squadMembersAsString.Contains(sup)) candidates.Add(sup);
            }

            //return the json of all squads
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(candidates);
            return jsonString;
        }




        /*****************************************************************************************************
         *POST: api/values. Takes application/json data for a new superhero and adds it to the database.
         *Here is an example of what the request body should look like:
              { "alias": "Superman", "origin": "Metropolis", "approvalRate": "11&", "archenemy": "Batman"}
        *****************************************************************************************************/
        [HttpPost]
        [Route("superhero")]
        public IActionResult Post([FromBody] Superhero newSuperHeroAsJson)
        {
            //validate the data send
            if(newSuperHeroAsJson.Alias != null && newSuperHeroAsJson.Alias != "")
            {
                var data = (from d in _context.Users where d.Alias == newSuperHeroAsJson.Alias select d.Alias).Count();
                if (data == 0)
                {
                    //Add the new super hero to the in-memory database
                    _context.Add(new Superhero
                    {
                        Alias = newSuperHeroAsJson.Alias ?? null,
                        Origin = newSuperHeroAsJson.Origin ?? null,
                        ApprovalRate = newSuperHeroAsJson.ApprovalRate ?? null,
                        Archenemy = newSuperHeroAsJson.Archenemy ?? null
                    });
                    _context.SaveChanges();
                    return CreatedAtAction("Get", new { Success = "New superhero successfully added" });
                }
                
            }

            return CreatedAtAction("Get", new {  Error = ": superhero name already in user"});
        }


        // PUT: api/superhero/{alias}
        //First param is the alias of the superhero. Thie superhero's archenemy is set to the value of the second
        //parameter provided that it is a valid superhero alias.  
        [HttpPut]
        [Route("superhero/{superheroAlias}/{villanAlias}")]
        public IActionResult Put(string superheroAlias, string villanAlias)
        {
            //First we check that alias and villanAlias are in the database
            var alias = (from d in _context.Users
                        where d.Alias == superheroAlias
                        select d.Alias).SingleOrDefault();
            if(alias == null)  return CreatedAtAction("Get", new { Error = ": superhero alias could not be found" });

            //Check the the villan we are updating is in the database
            var villan = (from d in _context.Users
                         where d.Alias != null && d.Alias == villanAlias
                         select d.Alias).SingleOrDefault();
            if (villan == null) return CreatedAtAction("Get", new { Error = ": villan alias could not be found" });      

            //Update the superhero to have the villan as archenemy
            var rowToUpdate = (from d in _context.Users
                               where d.Alias == alias
                               select d).SingleOrDefault();
            rowToUpdate.Archenemy = villan;
            _context.SaveChanges();

            //return sucess 
            return CreatedAtAction("Get", new { Success = ": superheros archenemy successfully updated" });
        }



         /************************************************************************************************
         * DELETE: api/superhero/{NameToDelete}
         * The path parameter is the alias of the superhero that will get deleted from the 
           database when this method is requested
         *************************************************************************************************/
        [HttpDelete()]
        [Route("superhero/{alias}")]
        public IActionResult Delete(string alias)
        {
            //Check if the row with that alias name exists
            var row = (from d in _context.Users
                      where d.Alias == alias
                      select d).FirstOrDefault();

            //If it exists, we delete it
            if(row != null)
            {
                _context.Users.Remove(row);
                _context.SaveChanges();
                return CreatedAtAction("Get", new { Success = ": superhero successfully deleted" });
            }

            return CreatedAtAction("Get", new { Error = ": superhero could not be found" });
        }




        /************************************************************************************************
         * Creates a squadron from an json array containing a squadron name and a list of superheros.
         *  Here is an example of how to format the body of the request.:
            body {
                  "name": "test2",
                  "squad": [{"Alias":"Batman"}, {"Alias":"Lex Luthor"}]
                 }
         *****************************************************************************************/
        [HttpPost]
        [Route("squadron")]
        public IActionResult Post([FromBody] Squadron superheroList)
        {
            //Check that the squadren name does not already exist
            var nameCheck = (from d in _context.Squad
                             where d.name == superheroList.name
                             select d).SingleOrDefault();

            //return error if squadron already exists
            if (nameCheck != null) return CreatedAtAction("Get", new { Error = ": Squadron already exists" });

            //get a list of all corrisponding villans of each superhero in the list.
            //Note that this works since no superhero can be their own villan
            List<string> villianList = new List<string>();
            foreach (var super in superheroList.Squad)
            {             
                //check that each superhero is valid
                var validSuperhero = (from d in _context.Users
                                      where d.Alias == super.Alias
                                      select d.Alias).SingleOrDefault();

                if(validSuperhero == null)
                    return CreatedAtAction("Get", new { Error = ": One or more of the superheros could not be found" });

                //Query for a villain to the current super hero
                var villian = (from d in _context.Users
                              where d.Alias == super.Alias
                               select d.Archenemy).SingleOrDefault();

                //If the superhero has a villain, add it to the list
                if (villian != null) villianList.Add(villian);
            }

            //check that no superhero has their villian in the group
            foreach(var superhero in superheroList.Squad)
            {
                
                for (int i = 0; i < villianList.Count(); i++) 
                {
                    //Check that the given superhero has no villans in the list
                    if (superhero.Alias == villianList[i])
                    {
                        //If invalid paring, return error message
                        string errorMesg = ": Could not create squadron becuase a superhero was assigned to the same squad " +
                                            "as their archenemy";
                        return CreatedAtAction("Get", new { Error = errorMesg });
                    }
                }
            }

            //create a new squadron
            _context.Add(new Squadron
            {
                Squad = superheroList.Squad,
                name = superheroList.name
            });

            //Save the new squatron to the database
            _context.SaveChanges();

            //return sucess message
            return CreatedAtAction("Get", new { Success = ": squadron successfully created" });
        }
    }
}
