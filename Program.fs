module Program 

open System
open System.Windows.Forms
open Microsoft.Data.Sqlite
open MySql.Data.MySqlClient // Import the MySQL library
open System.Drawing
open imperative.Program 


// Main Form 
let mainForm () =
    // Initialize the form
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    // Create Sign and Admin buttons
    let imperativeButton = new Button(Text = "Imperative", AutoSize = true, Location = Point(50, 50), BackColor = Color.Blue , ForeColor = Color.White , Font = new Font("sans", 19.0f) )
    let functionalButton = new Button(Text = "Functional", AutoSize = true, Location = Point(50, 150), BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 19.0f) )

    imperativeButton.Click.Add(fun _ ->
        imperative.Program.mainForm()
     )
    

    // Add controls to the form
    form.Controls.Add(imperativeButton)
    form.Controls.Add(functionalButton)

    // Display the form
    Application.Run(form)

// Entry point of the program
[<EntryPoint>]
let main argv =
    mainForm ()
    0