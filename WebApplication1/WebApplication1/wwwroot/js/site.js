document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id === "auth-form") {
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
            return;
        }

        // Генерація `credentials` для авторизації
        const credentials = btoa(loginInput.value + ':' + password);
        console.log(credentials);

        // RFC 7617 (Реальний запит до сервера можна розкоментувати)
        // fetch("", {
        //     method: "GET",
        //     headers: {
        //         'Authorization': 'Basic ' + credentials
        //     }
        // }).then(r => r.json()).then(console.log);
    }
});