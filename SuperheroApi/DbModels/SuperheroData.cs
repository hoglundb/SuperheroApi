using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


//Holds all objects that will be used by Entity Framework to generate an in-memory database.
namespace SuperheroApi.DbModels
{

    //Holds the data pertaining to a individual superhero
    public class Superhero
    {
        [Key]
        public string Alias { get; set; }
        public string Origin { get; set; }
        public string ApprovalRate { get; set; }
        public string Archenemy { get; set; }
    }


    //A squadron obj contains a list of superhero names and a squadron name
    public class Squadron
    {
        public Squadron()
        {
            Squad = new List<Name>();
        }
        [Key]
        public string name { get; set; }

        //Don't need this obj mapped anywhere. Entity framework will nuke everything if this isn't here
        public List<Name> Squad { get; set; }
    }
    
    //A superhero name. Used in building the squadron object
    public class Name
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int  id { get; set; }
        public string Alias { get; set; }
    }
}
