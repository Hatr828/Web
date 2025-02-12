document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id === "auth-form") {
        console.log("here")

        e.preventDefault();

        const loginInput = form.querySelector('[name="UserLogin"]');
        const passwordInput = form.querySelector('[name="Password"]');
        const errorContainer = document.getElementById('auth-errors');

        let errors = []; 

 
        errorContainer.textContent = "";

        if (loginInput.value.trim() === "") {
            errors.push("Логін не може бути порожнім.");
        }

        const password = passwordInput.value;
        if (password.length < 8) {
            errors.push("Пароль повинен бути не менше 8 символів.");
        }
        if (!/[A-Za-z]/.test(password)) {
            errors.push("Пароль повинен містити хоча б одну літеру.");
        }
        if (!/\d/.test(password)) {
            errors.push("Пароль повинен містити хоча б одну цифру.");
        }
        if (!/[\W_]/.test(password)) {
            errors.push("Пароль повинен містити хоча б один спеціальний символ.");
        }


        if (errors.length > 0) {
            errorContainer.innerHTML = errors.join("<br>");
           
        }

        // Генерація `credentials` для авторизації

        // RFC 7617
        const credentials = btoa(loginInput.value + ':' + passwordInput.value);
        fetch("/User/Authenticate", {
            method: "GET",
            headers: {
                'Authorization': 'Basic ' + credentials
            }
        }).then(r => {
            console.log(r);
            if (r.ok) {
                window.location.reload();
            }
            else {
                r.json().then(j => {
                    alert(j);   // Д.З.
                });
            }
        });

        console.log(credentials);   
    }
});

const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))