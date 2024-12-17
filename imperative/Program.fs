module  imperative.Program 

open System
open System.Windows.Forms
open Microsoft.Data.Sqlite
open MySql.Data.MySqlClient // Import the MySQL library
open System.Drawing
open Sign
open Connection

// Main Form 
let mainForm () =
    // Initialize the form
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    // Create Sign and Admin buttons
    let userButton = new Button(Text = "User", AutoSize = true, Location = Point(50, 50), BackColor = Color.Blue , ForeColor = Color.White , Font = new Font("sans", 19.0f) )
    let adminButton = new Button(Text = "Admin", AutoSize = true, Location = Point(150, 50), BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 19.0f) )

    // Event handler for User button
    userButton.Click.Add(fun _ ->
     Sign.LoginForm "User"
     )

    // Event handler for Admin button
    adminButton.Click.Add(fun _ -> 
    Sign.LoginForm "Admin"
    )

    // Add controls to the form
    form.Controls.Add(userButton)
    form.Controls.Add(adminButton)

    // Display the form
    form.Show()
