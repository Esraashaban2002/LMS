module Sign

open System
open System.Windows.Forms
open System.Drawing
open Microsoft.Data.Sqlite
open MySql.Data.MySqlClient 
open Connection
open AdminPanel
open UserPanel


// Register Form
let RegisterForm (expectedRole: string) =
     
    // Initialize the form
    let form = new Form(Text = "Register", AutoSize = true, BackColor = Color.White)

    // Create form controls
    let namelLabel = new Label(Text = "Name:", Location = Point(10, 10), AutoSize = true)
    let nameTextBox = new TextBox(Location = Point(100, 10), Width = 200)

    let emailLabel = new Label(Text = "Email:", Location = Point(10, 40), AutoSize = true)
    let emailTextBox = new TextBox(Location = Point(100, 40), Width = 200)

    let passwordLabel = new Label(Text = "Password:", Location = Point(10, 80), AutoSize = true)
    let passwordTextBox = new TextBox(Location = Point(100, 80), Width = 200, PasswordChar = '*')

    let statusLabel = new Label(Location = Point(10, 150), Width = 400, Height = 30)

    let registerButton = new Button(Text = "Register", AutoSize = true, Location = Point(200, 110), ForeColor = Color.White , Font = new Font("sans", 19.0f) )

    if expectedRole = "Admin" then
        registerButton.BackColor <- Color.Pink
    elif expectedRole = "User" then
        registerButton.BackColor <- Color.Blue
    else
        registerButton.BackColor <- Color.Gray 


    // Event handler for Register
    registerButton.Click.Add(fun _ ->
         Connection.registerUser expectedRole nameTextBox emailTextBox passwordTextBox statusLabel
    )



    // Add controls to the form
    form.Controls.Add(namelLabel)
    form.Controls.Add(nameTextBox)
    form.Controls.Add(emailLabel)
    form.Controls.Add(emailTextBox)
    form.Controls.Add(passwordLabel)
    form.Controls.Add(passwordTextBox)
    form.Controls.Add(statusLabel)
    form.Controls.Add(registerButton)

    // Display the form
    form.Show()


// Login Form
let LoginForm (expectedRole: string) =
    // Initialize the form
    let form = new Form(Text = "Login", AutoSize = true, BackColor = Color.White)

    // Create form controls
    let emailLabel = new Label(Text = "Email:", Location = Point(10, 10), AutoSize = true)
    let emailTextBox = new TextBox(Location = Point(100, 10), Width = 200)

    let passwordLabel = new Label(Text = "Password:", Location = Point(10, 40), AutoSize = true)
    let passwordTextBox = new TextBox(Location = Point(100, 40), Width = 200, PasswordChar = '*')

    let statusLabel = new Label(Location = Point(10, 80), Width = 400, Height = 30)

    let regLabel = new Label(Text="if you don't have account Create new Account" ,Location = Point(10, 160), Width = 400, Height = 30 ,ForeColor = Color.Blue)

    let loginButton = new Button(Text = "Login", AutoSize = true, Location = Point(200, 110), BackColor = Color.Blue , ForeColor = Color.White , Font = new Font("sans", 19.0f) )

    if expectedRole = "Admin" then
        loginButton.BackColor <- Color.Pink
        true
    else 
        loginButton.BackColor <- Color.Blue 
        false

    // Event handler for login
    loginButton.Click.Add(fun _ ->
        let result = Connection.login expectedRole emailTextBox passwordTextBox statusLabel
        if (result) then
            if (expectedRole = "Admin") then
                AdminPanel.createForm () 
                form.Close()  
            else  
                UserPanel.createForm () 
                form.Close()     
    )

    regLabel.Click.Add(fun _ ->
        RegisterForm expectedRole
    )


    // Add controls to the form
    form.Controls.Add(emailLabel)
    form.Controls.Add(emailTextBox)
    form.Controls.Add(passwordLabel)
    form.Controls.Add(passwordTextBox)
    form.Controls.Add(statusLabel)
    form.Controls.Add(regLabel)
    form.Controls.Add(loginButton)

    // Display the form
    form.Show()

