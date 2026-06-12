// Example starter JavaScript for disabling form submissions if there are invalid fields
(() => {
    'use strict'

    // Fetch all the forms we want to apply custom Bootstrap validation styles to
    const forms = document.querySelectorAll('.needs-validation')

    // Loop over them and prevent submission
    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault()
                event.stopPropagation()
            }

            form.classList.add('was-validated')
        }, false)
    })
})();

const modularRegistrationWindow = document.getElementById("modularRegistrationWindow");
const avatarInput = modularRegistrationWindow.querySelector("#avatar");
avatarInput.addEventListener("input", () => {
    if (avatarInput.files && avatarInput.files[0]) {
        modularRegistrationWindow.querySelector(".avatar").style.backgroundImage = `url(${URL.createObjectURL(avatarInput.files[0])})`;
    }
});

const postModal = document.getElementById("postModal");
if (postModal) {
    const previewInput = postModal.querySelector("#previewInput");
    previewInput.addEventListener("input", () => {
        if (previewInput.files && previewInput.files[0]) {
            postModal.querySelector(".preview").style.backgroundImage = `url(${URL.createObjectURL(previewInput.files[0])})`;
        }
    });
}

if (postModal) {
    function addTile(input, container) {
        const value = input.value.trim();
        if (!value) return;
        for (let elem of container.querySelectorAll(".tile")) {
            if (elem.textContent === value) return;
        }
        const tile = document.createElement("span");
        tile.textContent = value;
        tile.classList.add("tile");

        let inputName = "";
        if (container.id === "techContainer") inputName = "technologiesAndTools";
        else if (container.id === "linkContainer") inputName = "links";
        else if (container.id === "embeddedLinkContainer") inputName = "embeddedLinks";

        const hiddenInput = document.createElement("input");
        hiddenInput.type = "hidden";
        hiddenInput.name = inputName;
        hiddenInput.value = value;

        tile.appendChild(hiddenInput);

        container.appendChild(tile);

        tile.addEventListener("click", () => tile.remove());
        input.value = "";
    }

    const techInput = postModal.querySelector("#techInput");
    const btnAddTech = postModal.querySelector("#btnAddTech");
    const techContainer = postModal.querySelector("#techContainer");

    techInput.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            e.preventDefault();
            addTile(techInput, techContainer);
        }
    });
    btnAddTech.addEventListener("click", () => addTile(techInput, techContainer));

    const linkInput = postModal.querySelector("#linkInput");
    const btnAddLink = postModal.querySelector("#btnAddLink");
    const linkContainer = postModal.querySelector("#linkContainer");

    linkInput.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            e.preventDefault();
            addTile(linkInput, linkContainer);
        }
    });
    btnAddLink.addEventListener("click", () => addTile(linkInput, linkContainer));

    const embeddedLinkInput = postModal.querySelector("#embeddedLinkInput");
    const btnAddembeddedLink = postModal.querySelector("#btnAddembeddedLink");
    const embeddedLinkContainer = postModal.querySelector("#embeddedLinkContainer");

    embeddedLinkInput.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            e.preventDefault();
            addTile(embeddedLinkInput, embeddedLinkContainer);
        }
    });
    btnAddembeddedLink.addEventListener("click", () => addTile(embeddedLinkInput, embeddedLinkContainer));

    postModal.querySelectorAll(".tile").forEach(tile => {
        tile.addEventListener("click", () => tile.remove());
    });
}

// =========================================================================
// ЄДИНИЙ КОНТРОЛЕР СТОРІНКИ ПОСТІВ (ПОШУК, ФІЛЬТРИ, МОДАЛКИ ТА ЗАГЛУШКИ)
// =========================================================================
document.addEventListener("DOMContentLoaded", () => {

    // --- 1. ОЧИЩЕННЯ ЗАГЛУШОК (ЗБЕРЕЖЕННЯ КАРТКИ СТВОРЕННЯ ПОСТУ) ---
    // Видаляємо лише тимчасові скелетони та заглушки карт контенту
    const dynamicPlugs = document.querySelectorAll(".skeleton, .skeleton-container, .plug-card");
    dynamicPlugs.forEach(el => el.remove());

    // Видаляємо елементи з класами placeholder, але ПОВНІСТЮ ігноруємо кнопку ".new-post"
    const placeholders = document.querySelectorAll("[class*='placeholder']");
    placeholders.forEach(el => {
        if (!el.classList.contains("new-post") && !el.closest(".new-post")) {
            el.remove();
        }
    });


    // --- 2. КЕРУВАННЯ ДЕТАЛЬНИМ МОДАЛЬНИМ ВІКНОМ ТА URL (?post=id) ---
    const postCards = document.querySelectorAll(".post-card");
    postCards.forEach(card => {
        card.addEventListener("click", (e) => {
            // Якщо клік відбувся по кнопках керування (редагувати/видалити) — не відкриваємо деталі
            if (e.target.closest('.btn-control') || e.target.closest('.delete-post-trigger-btn') || e.target.closest('.edit-post-btn')) {
                return;
            }

            // Зупиняємо стандартний тригер Bootstrap (data-bs-toggle), щоб не було дублювання
            e.preventDefault();

            const postId = card.getAttribute("data-post-id");
            const modalEl = document.getElementById(`postModal-${postId}`);
            if (modalEl) {
                // Перевіряємо, чи модалка вже не відкрита (щоб уникнути double-open)
                const isOpened = modalEl.classList.contains('show') || modalEl.classList.contains('collapsing');
                if (!isOpened) {
                    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
                    modal.show();
                }
            }
        });
    });

    // Відслідковування відкриття/закриття модалок для відображення в URL
    const modalElements = document.querySelectorAll(".modal[id^='postModal-']");
    modalElements.forEach(modalEl => {
        modalEl.addEventListener("show.bs.modal", () => {
            const id = modalEl.id.replace("postModal-", "");
            const url = new URL(window.location.href);
            url.searchParams.set("post", id);
            history.pushState(null, "", url.toString());
        });

        modalEl.addEventListener("hide.bs.modal", () => {
            const url = new URL(window.location.href);
            url.searchParams.delete("post");
            history.pushState(null, "", url.toString());
        });
    });

    // Автоматичне відкриття модалки при прямиму переході за посиланням з URL-параметром
    const urlParams = new URLSearchParams(window.location.search);
    const initialPostId = urlParams.get("post");
    if (initialPostId) {
        const targetModalEl = document.getElementById(`postModal-${initialPostId}`);
        if (targetModalEl) {
            window.addEventListener("load", () => {
                const modal = bootstrap.Modal.getOrCreateInstance(targetModalEl);
                modal.show();
            });
        }
    }


    // --- 3. ЛОГІКА ВІДОБРАЖЕННЯ ТА РОБОТИ КНОПКИ "СКИНУТИ ФІЛЬТРИ" ---
    const filterForm = document.getElementById("filterForm");
    const resetFiltersBtn = document.getElementById("resetFiltersBtn");

    function toggleResetButtonVisibility() {
        if (!filterForm || !resetFiltersBtn) return;

        // Перевірка активності будь-яких чекбоксів
        const hasCheckedCheckboxes = filterForm.querySelectorAll("input[type='checkbox']:checked").length > 0;

        // Перевірка радіокнопок періоду часу (ігноруємо значення за замовчуванням "За весь час")
        const checkedRadios = filterForm.querySelectorAll("input[type='radio']:checked");
        let hasActiveTimeFilter = false;

        checkedRadiresLoop: for (let radio of checkedRadios) {
            const val = radio.value ? radio.value.toLowerCase() : "";
            const id = radio.id ? radio.id.toLowerCase() : "";

            // Якщо обрано будь-що крім "all", "alltime" чи порожнього значення — фільтр вважається активним
            if (val !== "" && val !== "all" && val !== "alltime" && !id.includes("all")) {
                hasActiveTimeFilter = true;
                break checkedRadiresLoop;
            }
        }

        // Відображаємо або ховаємо кнопку відповідно до умов
        if (hasCheckedCheckboxes || hasActiveTimeFilter) {
            resetFiltersBtn.style.setProperty("display", "block", "important");
        } else {
            resetFiltersBtn.style.setProperty("display", "none", "important");
        }
    }

    // Перевірка стану фільтрів під час першого завантаження сторінки
    toggleResetButtonVisibility();

    if (filterForm) {
        filterForm.addEventListener("change", toggleResetButtonVisibility);
    }

    if (resetFiltersBtn) {
        resetFiltersBtn.addEventListener("click", () => {
            if (filterForm) {
                // Повне очищення полів форми
                filterForm.querySelectorAll("input[type='checkbox']").forEach(el => el.checked = false);
                filterForm.querySelectorAll("input[type='radio']").forEach(el => el.checked = false);

                // Перенаправлення на чисту сторінку без параметрів
                window.location.href = window.location.pathname;
            }
        });
    }


    // --- 4. СИНХРОНІЗОВАНА СТРУКТУРА ПОШУКУ ТА ФІЛЬТРАЦІЇ (UNIFIED SEARCH) ---
    const searchForm = document.getElementById("searchForm");
    const sortForm = document.getElementById("sortForm");
    const searchInput = document.getElementById("postSearchInput");
    const searchIcon = document.getElementById("searchIcon");

    // Забезпечуємо стабільний вигляд іконки пошуку без динамічних підмін на хрестики
    if (searchIcon) {
        searchIcon.className = "icon-search";
    }

    const formSubmitHandler = (e) => {
        e.preventDefault();
        executeUnifiedSearch();
    };

    // Прив'язуємо єдиний обробник подій до всіх керуючих форм сторінки
    if (searchForm) searchForm.addEventListener("submit", formSubmitHandler);
    if (filterForm) filterForm.addEventListener("submit", formSubmitHandler);
    if (sortForm) sortForm.addEventListener("submit", formSubmitHandler);

    function executeUnifiedSearch() {
        const url = new URL(window.location.origin + window.location.pathname);

        // Збір значення з пошукового інпуту
        if (searchInput && searchInput.value.trim() !== "") {
            url.searchParams.set("searchString", searchInput.value.trim());
        }

        // Збір обраних типів спеціальності
        document.querySelectorAll("input[name='selectedTypes']:checked").forEach(cb => {
            url.searchParams.append("selectedTypes", cb.value);
        });

        // Збір обраних інструментів/технологій
        document.querySelectorAll("input[name='selectedTechs']:checked").forEach(cb => {
            url.searchParams.append("selectedTechs", cb.value);
        });

        // Збір часового проміжку
        const timeRadio = document.querySelector("input[name='timePeriod']:checked");
        if (timeRadio && timeRadio.value !== "all" && timeRadio.value !== "alltime") {
            url.searchParams.set("timePeriod", timeRadio.value);
        }

        // Збір критерію сортування карток
        const sortRadio = document.querySelector("input[name='sortingCriterions']:checked");
        if (sortRadio) {
            url.searchParams.set("sortBy", sortRadio.value);
        }

        // Повернення на першу сторінку пагінації при нових умовах вибірки
        url.searchParams.set("page", "1");

        window.location.href = url.toString();
    }
});


// =========================================================================
// ДЕЛЕГУВАННЯ ПОДІЙ ДЛЯ ДИНАМІЧНИХ МОДАЛОК (СТВОРЕННЯ / РЕДАГУВАННЯ ПОСТІВ)
// =========================================================================
document.addEventListener("click", function (e) {
    const editBtn = e.target.closest(".edit-post-btn");
    const createBtn = e.target.closest("[data-bs-target='#postModal']:not(.edit-post-btn)");
    const postModalEl = document.getElementById("postModal");

    if (!postModalEl) return;
    const modalTitleEl = postModalEl.querySelector("#postModalLabel");

    if (editBtn) {
        if (modalTitleEl) modalTitleEl.textContent = "Редагування посту";

        const idInput = postModalEl.querySelector("#modalPostId");
        if (idInput) idInput.value = editBtn.dataset.id;

        const titleInput = postModalEl.querySelector("[name='title']");
        if (titleInput) titleInput.value = editBtn.dataset.title || "";

        const descInput = postModalEl.querySelector("[name='description']");
        if (descInput) descInput.value = editBtn.dataset.description || "";

        const typeInput = postModalEl.querySelector("[name='type']");
        if (typeInput) typeInput.value = editBtn.dataset.type || "";

        const previewEl = postModalEl.querySelector(".preview");
        if (previewEl && editBtn.dataset.preview) {
            previewEl.style.backgroundImage = `url(${editBtn.dataset.preview})`;
        }

        ["techContainer", "linkContainer", "embeddedLinkContainer"].forEach(id => {
            const container = document.getElementById(id);
            if (container) container.innerHTML = "";
        });

        if (editBtn.dataset.tech) {
            editBtn.dataset.tech.split(",").filter(t => t.trim()).forEach(t => appendSavedTile(t, document.getElementById("techContainer"), "technologiesAndTools"));
        }
        if (editBtn.dataset.links) {
            editBtn.dataset.links.split(",").filter(l => l.trim()).forEach(l => appendSavedTile(l, document.getElementById("linkContainer"), "links"));
        }
        if (editBtn.dataset.embedded) {
            editBtn.dataset.embedded.split(",").filter(el => el.trim()).forEach(el => appendSavedTile(el, document.getElementById("embeddedLinkContainer"), "embeddedLinks"));
        }

        const submitBtn = postModalEl.querySelector("button[type='submit']");
        if (submitBtn) submitBtn.textContent = "Зберегти зміни";
    }
    else if (createBtn) {
        if (modalTitleEl) modalTitleEl.textContent = "Створення посту";

        const idInput = postModalEl.querySelector("#modalPostId");
        if (idInput) idInput.value = "0";

        const form = postModalEl.querySelector("form");
        if (form) form.reset();

        const previewEl = postModalEl.querySelector(".preview");
        if (previewEl) previewEl.style.backgroundImage = "url('/images/default-post-preview.jpg')";

        ["techContainer", "linkContainer", "embeddedLinkContainer"].forEach(id => {
            const container = document.getElementById(id);
            if (container) container.innerHTML = "";
        });

        const submitBtn = postModalEl.querySelector("button[type='submit']");
        if (submitBtn) submitBtn.textContent = "Створити";
    }
});

function appendSavedTile(value, container, inputName) {
    if (!container || !value.trim()) return;
    const tile = document.createElement("span");
    tile.textContent = value;
    tile.classList.add("tile");

    const hiddenInput = document.createElement("input");
    hiddenInput.type = "hidden";
    hiddenInput.name = inputName;
    hiddenInput.value = value;
    tile.appendChild(hiddenInput);

    container.appendChild(tile);
    tile.addEventListener("click", () => tile.remove());
}

// Передача ID до модалки підтвердження видалення
document.addEventListener("click", function (e) {
    const deleteTrigger = e.target.closest(".delete-post-trigger-btn");
    if (deleteTrigger) {
        const deleteInput = document.getElementById("deletePostId");
        if (deleteInput) {
            deleteInput.value = deleteTrigger.dataset.id;
        }
    }
});

document.addEventListener("click", (e) => {
    const copyButton = e.target.closest(".btn-copy-link");

    if (copyButton) {
        // Отримуємо ID поста
        const postId = copyButton.getAttribute("data-post-id");

        if (postId) {
            // window.location.origin автоматично підставить http://localhost:7277 або https://studfolio.com
            const urlToCopy = `${window.location.origin}/Posts?post=${postId}`;

            navigator.clipboard.writeText(urlToCopy)
                .then(() => {
                    copyButton.classList.add("text-success");

                    setTimeout(() => {
                        copyButton.classList.remove("text-success");
                    }, 1500);
                })
                .catch(err => {
                    console.error("Не вдалося скопіювати посилання: ", err);
                });
        }
    }
});

const portfolioContainer = document.getElementById("portfolioContainer")

const modalHTML = `
<div class="modal fade" id="sectionSelectionModal" tabindex="-1" aria-labelledby="modalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <p class="modal-title text-primary fs-5" id="modalLabel">Створення секції</p>
                <button type="button" class="btn-icon" data-bs-dismiss="modal" aria-label="Закрити">
                    <span class="icon-dagger"></span>
                </button>
            </div>
            <div class="modal-body">
                <div class="list-group">
                    <button type="button" class="list-group-item list-group-item-action" data-select="story">Розповідь</button>
                    <button type="button" class="list-group-item list-group-item-action" data-select="posts">Список постів</button>
                    <button type="button" class="list-group-item list-group-item-action" data-select="post">Пост</button>
                </div>
            </div>
        </div>
    </div>
</div>`;

let sectionModal;
let bsSectionModal;
let currentActivePlug = null;
let isChoiceMade = false;

function createPortfolioSection(plugBlock, plugName) {
    if (plugName === "plug-add-preamble") {
        plugBlock.className = "portfolio-preamble-section";

        // Початкова структура (поки завантажуються дані, показуємо порожні або дефолтні блоки)
        plugBlock.innerHTML = `
            <div class="avatar" id="portfolioAvatarDiv">
                <div class="btn-camera">
                    <input class="form-control" type="file" id="portfolioAvatarInput" accept="image/*">
                    <span class="icon-camera"></span>
                </div>
            </div>
            <h1 id="portfolioUserName">Завантаження...</h1>
            <p class="plug-card text-center text-accent fs-4" id="specialtyTextBtn"></p>
            <p class="plug-card text-center fs-4" id="eduInput" contenteditable="true"></p>
        `;

        const avatarDiv = plugBlock.querySelector("#portfolioAvatarDiv");
        const nameH1 = plugBlock.querySelector("#portfolioUserName");
        const specialtyInput = plugBlock.querySelector("#specialtyTextBtn");
        const eduInput = plugBlock.querySelector("#eduInput");

        // --- ДИНАМІЧНЕ ПІДТЯГУВАННЯ ДАНИХ З СЕРВЕРА ---
        fetch('/MyPortfolio/GetUserData')
            .then(res => res.json())
            .then(data => {
                // Встановлюємо ПІБ користувача
                nameH1.textContent = data.name || "Користувач";

                // Якщо є збережений аватар, робимо його фоном
                if (data.avatar) {
                    avatarDiv.style.backgroundImage = `url(${data.avatar})`;
                }

                // Перевірка спеціальності: якщо є в БД — виводимо та прибираємо сіру заглушку
                if (data.specialty && data.specialty.trim() !== "") {
                    specialtyInput.textContent = data.specialty;
                    specialtyInput.classList.remove("plug-card");
                } else {
                    specialtyInput.textContent = "Натисніть, щоб обрати спеціальність";
                    specialtyInput.classList.add("plug-card");
                }

                // Перевірка закладу освіти: якщо є в БД — виводимо та прибираємо сіру заглушку
                if (data.education && data.education.trim() !== "") {
                    eduInput.textContent = data.education;
                    eduInput.classList.remove("plug-card");
                } else {
                    eduInput.textContent = "Введіть назву закладу освіти...";
                    eduInput.classList.add("plug-card");
                }
            })
            .catch(err => {
                console.error("Не вдалося завантажити дані:", err);
                nameH1.textContent = "Помилка завантаження";
            });

        const pAvatarInput = plugBlock.querySelector("#portfolioAvatarInput");
        pAvatarInput.addEventListener("input", () => {
            if (pAvatarInput.files && pAvatarInput.files[0]) {
                avatarDiv.style.backgroundImage = `url(${URL.createObjectURL(pAvatarInput.files[0])})`;
            }
        });

        // --- ЛОГІКА СПЕЦІАЛЬНОСТІ (ЧЕРЕЗ МОДАЛКУ) ---
        specialtyInput.addEventListener("click", () => {
            let specialtySelectionModal = document.getElementById("specialtySelectionModal");

            if (!specialtySelectionModal) {
                document.body.insertAdjacentHTML('beforeend', `
                    <div class="modal fade" id="specialtySelectionModal" tabindex="-1" aria-labelledby="specialtySelectionModalLabel" aria-hidden="true">
                        <div class="modal-dialog modal-dialog-centered">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <p class="modal-title text-primary fs-5" id="specialtySelectionModalLabel">Вибір спеціальності</p>
                                    <button type="button" class="btn-icon" data-bs-dismiss="modal" aria-label="Закрити">
                                        <span class="icon-dagger"></span>
                                    </button>
                                </div>

                                <div class="modal-body">
                                    <select class="form-select py-3 border-primary" aria-label="Вибір спеціальності" id="portfolioSpecialty">
                                        <option value="" selected disabled hidden>Виберіть спеціальність</option>
                                        <optgroup label="A Освіта">
                                            <option value="A1 Освітні науки">A1 Освітні науки</option>
                                            <option value="A2 Дошкільна освіта">A2 Дошкільна освіта</option>
                                            <option value="A3 Початкова освіта">A3 Початкова освіта</option>
                                            <option value="A4 Середня освіта (за предметними спеціальностями)">A4 Середня освіта (за предметними спеціальностями)</option>
                                            <option value="A5 Професійна освіта (за спеціалізаціями)">A5 Професійна освіта (за спеціалізаціями)</option>
                                            <option value="A6 Спеціальна освіта">A6 Спеціальна освіта</option>
                                            <option value="A7 Фізична культура і спорт">A7 Фізична культура і спорт</option>
                                        </optgroup>

                                        <optgroup label="B Культура, мистецтво та гуманітарні науки">
                                            <option value="B1 Аудіовізуальне мистецтво та медіавиробництво">B1 Аудіовізуальне мистецтво та медіавиробництво</option>
                                            <option value="B2 Дизайн">B2 Дизайн</option>
                                            <option value="B3 Декоративне мистецтво та ремесла">B3 Декоративне мистецтво та ремесла</option>
                                            <option value="B4 Образотворче мистецтво та реставрація">B4 Образотворче мистецтво та реставрація</option>
                                            <option value="B5 Музичне мистецтво">B5 Музичне мистецтво</option>
                                            <option value="B6 Перформативні мистецтва">B6 Перформативні мистецтва</option>
                                            <option value="B7 Релігієзнавство">B7 Релігієзнавство</option>
                                            <option value="B8 Богослов’я">B8 Богослов’я</option>
                                            <option value="B9 Історія та археологія">B9 Історія та археологія</option>
                                            <option value="B10 Філософія">B10 Філософія</option>
                                            <option value="B11 Філологія (за спеціалізаціями)">B11 Філологія (за спеціалізаціями)</option>
                                            <option value="B12 Культурологія та музеєзнавство">B12 Культурологія та музеєзнавство</option>
                                            <option value="B13 Бібліотечна, інформаційна та архівна справа">B13 Бібліотечна, інформаційна та архівна справа</option>
                                            <option value="B14 Організація соціокультурної діяльності">B14 Організація соціокультурної діяльності</option>
                                        </optgroup>

                                        <optgroup label="C Соціальні науки, журналістика та інформація">
                                            <option value="C1 Економіка">C1 Економіка</option>
                                            <option value="C2 Політологія">C2 Політологія</option>
                                            <option value="C3 Міжнародні відносини">C3 Міжнародні відносини</option>
                                            <option value="C4 Психологія">C4 Психологія</option>
                                            <option value="C5 Соціологія">C5 Соціологія</option>
                                            <option value="C6 Географія та регіональні студії">C6 Географія та регіональні студії</option>
                                            <option value="C7 Журналістика">C7 Журналістика</option>
                                        </optgroup>

                                        <optgroup label="D Бізнес, адміністрування та право">
                                            <option value="D1 Облік і оподаткування">D1 Облік і оподаткування</option>
                                            <option value="D2 Фінанси, банківська справа, страхування та фондовий ринок">D2 Фінанси, банківська справа, страхування та фондовий ринок</option>
                                            <option value="D3 Менеджмент">D3 Менеджмент</option>
                                            <option value="D4 Публічне управління та адміністрування">D4 Публічне управління та адміністрування</option>
                                            <option value="D5 Маркетинг">D5 Маркетинг</option>
                                            <option value="D6 Секретарська та офісна справа">D6 Секретарська та офісна справа</option>
                                            <option value="D7 Торгівля">D7 Торгівля</option>
                                            <option value="D8 Право">D8 Право</option>
                                            <option value="D9 Міжнародне право">D9 Міжнародне право</option>
                                        </optgroup>

                                        <optgroup label="E Природничі науки, математика та статистика">
                                            <option value="E1 Біологія та біохімія">E1 Біологія та біохімія</option>
                                            <option value="E2 Екологія">E2 Екологія</option>
                                            <option value="E3 Хімія">E3 Хімія</option>
                                            <option value="E4 Науки про Землю">E4 Науки про Землю</option>
                                            <option value="E5 Фізика та астрономія">E5 Фізика та астрономія</option>
                                            <option value="E6 Прикладна фізика та наноматеріали">E6 Прикладна фізика та наноматеріали</option>
                                            <option value="E7 Математика">E7 Математика</option>
                                            <option value="E8 Статистика">E8 Статистика</option>
                                        </optgroup>

                                        <optgroup label="F Інформаційні технології">
                                            <option value="F1 Прикладна математика">F1 Прикладна математика</option>
                                            <option value="F2 Інженерія програмного забезпечення">F2 Інженерія програмного забезпечення</option>
                                            <option value="F3 Комп’ютерні науки">F3 Комп’ютерні науки</option>
                                            <option value="F4 Системний аналіз та наука про дані">F4 Системний аналіз та наука про дані</option>
                                            <option value="F5 Кібербезпека та захист інформації">F5 Кібербезпека та захист інформації</option>
                                            <option value="F6 Інформаційні системи і технології">F6 Інформаційні системи і технології</option>
                                            <option value="F7 Комп’ютерна інженерія">F7 Комп’ютерна інженерія</option>
                                        </optgroup>

                                        <optgroup label="G Інженерія, виробництво та будівництво">
                                            <option value="G1 Хімічні технології та інженерія">G1 Хімічні технології та інженерія</option>
                                            <option value="G2 Технології захисту навколишнього середовища">G2 Технології захисту навколишнього середовища</option>
                                            <option value="G3 Електрична інженерія">G3 Електрична інженерія</option>
                                            <option value="G4 Енерговиробництво (за спеціалізаціями)">G4 Енерговиробництво (за спеціалізаціями)</option>
                                            <option value="G5 Електроніка, електронні комунікації, приладобудування та радіотехніка">G5 Електроніка, електронні комунікації, приладобудування та радіотехніка</option>
                                            <option value="G6 Інформаційно-вимірювальні технології">G6 Інформаційно-вимірювальні технології</option>
                                            <option value="G7 Автоматизація, комп’ютерно-інтегровані технології та робототехніка">G7 Автоматизація, комп’ютерно-інтегровані технології та робототехніка</option>
                                            <option value="G8 Матеріалознавство">G8 Матеріалознавство</option>
                                            <option value="G9 Прикладна механіка">G9 Прикладна механіка</option>
                                            <option value="G10 Металургія">G10 Металургія</option>
                                            <option value="G11 Машинобудування">G11 Машинобудування</option>
                                            <option value="G12 Авіаційна та ракетно-космічна техніка">G12 Авіаційна та ракетно-космічна техніка</option>
                                            <option value="G13 Харчові технології">G13 Харчові технології</option>
                                            <option value="G14 Деревообробні та меблеві технології">G14 Деревообробні та меблеві технології</option>
                                            <option value="G15 Технології легкої промисловості">G15 Технології легкої промисловості</option>
                                            <option value="G16 Гірництво та нафтогазові технології">G16 Гірництво та нафтогазові технології</option>
                                            <option value="G17 Архітектура та містобудування">G17 Архітектура та містобудування</option>
                                            <option value="G18 Геодезія та землеустрій">G18 Геодезія та землеустрій</option>
                                            <option value="G19 Будівництво та цивільна інженерія">G19 Будівництво та цивільна інженерія</option>
                                            <option value="G20 Видавництво та поліграфія">G20 Видавництво та поліграфія</option>
                                            <option value="G21 Біотехнології та біоінженерія">G21 Біотехнології та біоінженерія</option>
                                            <option value="G22 Біомедична інженерія">G22 Біомедична інженерія</option>
                                        </optgroup>

                                        <optgroup label="H Сільське, лісове, рибне господарство та ветеринарна медицина">
                                            <option value="H1 Агрономія">H1 Агрономія</option>
                                            <option value="H2 Тваринництво">H2 Тваринництво</option>
                                            <option value="H3 Садово-паркове господарство">H3 Садово-паркове господарство</option>
                                            <option value="H4 Лісове господарство">H4 Лісове господарство</option>
                                            <option value="H5 Водні біоресурси та аквакультура">H5 Водні біоресурси та аквакультура</option>
                                            <option value="H6 Ветеринарна медицина">H6 Ветеринарна медицина</option>
                                            <option value="H7 Агроінженерія">H7 Агроінженерія</option>
                                        </optgroup>

                                        <optgroup label="J Транспорт та послуги">
                                            <option value="J1 Послуги краси">J1 Послуги краси</option>
                                            <option value="J2 Готельно-ресторанна справа та кейтеринг">J2 Готельно-ресторанна справа та кейтеринг</option>
                                            <option value="J3 Туризм і рекреація">J3 Туризм і рекреація</option>
                                            <option value="J4 Охорона праці">J4 Охорона праці</option>
                                            <option value="J5 Морський та внутрішній водний транспорт">J5 Морський та внутрішній водний транспорт</option>
                                            <option value="J6 Авіаційний транспорт">J6 Авіаційний транспорт</option>
                                            <option value="J7 Залізничний транспорт">J7 Залізничний транспорт</option>
                                            <option value="J8 Автомобільний транспорт">J8 Автомобільний транспорт</option>
                                        </optgroup>
                                        <optgroup label="K Безпека та оборона">
                                            <option value="K1 Державна безпека">K1 Державна безпека</option>
                                            <option value="K2 Безпека державного кордону">K2 Безпека державного кордону</option>
                                            <option value="K3 Національна безпека (за окремими сферами забезпечення і видами діяльності)">K3 Національна безпека (за окремими сферами забезпечення і видами діяльності)</option>
                                            <option value="K4 Управління інформаційною безпекою">K4 Управління інформаційною безпекою</option>
                                            <option value="K5 Військове управління (за видами збройних сил)">K5 Військове управління (за видами збройних сил)</option>
                                            <option value="K6 Забезпечення військ (сил)">K6 Забезпечення військ (сил)</option>
                                            <option value="K7 Озброєння та військова техніка">K7 Озброєння та військова техніка</option>
                                            <option value="K8 Пожежна безпека">K8 Пожежна безпека</option>
                                            <option value="K9 Правоохоронна діяльність">K9 Правоохоронна діяльність</option>
                                            <option value="K10 Цивільна безпека">K10 Цивільна безпека</option>
                                        </optgroup>
                                    </select>
                                </div>
                            </div>
                        </div>
                    </div>
                `);
                specialtySelectionModal = document.getElementById("specialtySelectionModal");
            }

            const bsSpecialtyModal = new bootstrap.Modal(specialtySelectionModal);
            bsSpecialtyModal.show();

            const portfolioSpecialty = specialtySelectionModal.querySelector("#portfolioSpecialty");
            portfolioSpecialty.onchange = function () {
                specialtyInput.textContent = this.value;
                specialtyInput.classList.remove("plug-card");
                bsSpecialtyModal.hide();
            };
        });

        // --- ЛОГІКА НАВЧАЛЬНОГО ЗАКЛАДУ (З ПЕРЕВІРКОЮ НА ПУСТОТУ) ---
        eduInput.addEventListener("focus", function () {
            this.classList.remove("plug-card");
        });

        eduInput.addEventListener("blur", function () {
            // Якщо завантажився/ввівся текст, а потім користувач його повністю стер — повертаємо заглушку
            if (this.textContent.trim() === "") {
                this.classList.add("plug-card");
            }
        });

        addPlugBlock("plug-add-section");
    } else if (plugName === "plug-add-section") {
        currentActivePlug = plugBlock;
        isChoiceMade = false;
        bsSectionModal.show();
    }
}

// Глобальна функція для ініціалізації вікна вибору посту
function openChoicePostModal(targetElement, sectionType) {
    let choicePostModal = document.getElementById("choicePostModal");

    // Створюємо модалку, якщо її ще немає в DOM
    if (!choicePostModal) {
        document.body.insertAdjacentHTML("beforeend", `
        <div class="modal fade" id="choicePostModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <p class="modal-title text-primary fs-5">Вибір посту</p>
                        <button type="button" class="btn-icon" data-bs-dismiss="modal" aria-label="Закрити"><span class="icon-dagger"></span></button>
                    </div>
                    <div class="modal-body">
                        <div class="d-flex flex-grow-1 mb-3">
                            <input class="form-control border-primary me-2" type="search" id="postSearchInput" placeholder="Вкажіть заголовок посту...">
                            <button class="btn-icon" type="button" id="postSearchBtn"><span class="icon-search"></span></button>
                        </div>
                        <div class="list-group" id="modalPostsList"></div>
                    </div>
                </div>
            </div>
        </div>`);
        choicePostModal = document.getElementById("choicePostModal");
    }

    const bsChoicePostModal = new bootstrap.Modal(choicePostModal);
    const searchInput = choicePostModal.querySelector("#postSearchInput");
    const searchBtn = choicePostModal.querySelector("#postSearchBtn");
    const postsList = choicePostModal.querySelector("#modalPostsList");

    async function loadPosts() {
        const query = searchInput.value.trim();
        postsList.innerHTML = `<div class="text-center p-3"><div class="spinner-border text-primary" role="status"></div></div>`;
        try {
            const response = await fetch(`/MyPortfolio/SearchPosts?query=${encodeURIComponent(query)}`);
            const posts = await response.json();
            postsList.innerHTML = "";
            if (posts.length === 0) {
                postsList.innerHTML = `<p class="text-muted text-center p-3">Постів не знайдено</p>`;
                return;
            }
            posts.forEach(post => {
                const btn = document.createElement("button");
                btn.type = "button";
                btn.className = "list-group-item list-group-item-action d-flex align-items-center gap-3";
                btn.innerHTML = `
                    <img src="${post.preview}" alt="" style="width: 40px; height: 40px; object-fit: cover; border-radius: 4px;">
                    <div class="text-start"><h6>${post.title}</h6><small class="text-muted">${post.type}</small></div>`;

                btn.addEventListener("click", () => {
                    handlePostSelection(post.postID, post.title, post.preview, targetElement, sectionType);
                    bsChoicePostModal.hide();
                });
                postsList.appendChild(btn);
            });
        } catch (error) {
            postsList.innerHTML = `<p class="text-danger text-center p-3">Помилка завантаження</p>`;
        }
    }

    searchBtn.onclick = loadPosts;
    searchInput.onkeyup = (e) => { if (e.key === "Enter") loadPosts(); };
    bsChoicePostModal.show();
    loadPosts();
}

// Функція, яка обробляє безпосереднє вставлення даних в сторінку портфоліо
function handlePostSelection(postId, title, preview, targetElement, sectionType) {
    if (sectionType === "posts") {
        // 1. Створюємо копію елемента БЕЗ старих обробників подій (true копіює і внутрішні теги)
        const cleanElement = targetElement.cloneNode(true);

        // 2. Налаштовуємо новий зовнішній вигляд та атрибути
        cleanElement.classList.remove("plug-card", "select-post-trigger");
        cleanElement.className = "portfolio-post-card-preview card";
        cleanElement.setAttribute("data-post-id", postId);
        cleanElement.innerHTML = `
            <img src="${preview}" class="card-img-top" alt="Обкладинка">
            <div class="card-body"><p class="card-title text-primary">${title}</p></div>`;

        // 3. Замінюємо старий елемент у DOM на наш новий чистий елемент
        targetElement.parentNode.replaceChild(cleanElement, targetElement);

        // 4. Тепер цей елемент реагує ТІЛЬКИ на відкриття перегляду поста, модалка вибору більше ніколи не вискочить
        cleanElement.addEventListener("click", (e) => {
            e.stopPropagation();
            const newUrl = new URL(window.location.href);
            newUrl.searchParams.set('post', postId);
            window.history.pushState({}, '', newUrl);
            openGlobalPostModal(postId);
        });
    }
    else if (sectionType === "post") {
        // Для одиночного поста показуємо індикатор завантаження
        targetElement.innerHTML = `<div class="text-center p-5 w-100"><div class="spinner-border text-primary" role="status"></div></div>`;

        fetch(`/MyPortfolio/GetPostModalPartial?id=${postId}`)
            .then(res => res.text())
            .then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, "text/html");
                const modalBody = doc.querySelector(".modal-body");

                if (modalBody) {
                    // 1. Клонуємо елемент, щоб повністю стерти клік вибору модалки
                    const cleanElement = targetElement.cloneNode(false);

                    // 2. Оновлюємо класи та контент
                    cleanElement.classList.remove("plug-card", "select-single-post-trigger");
                    cleanElement.className = "portfolio-post-full-content w-100 d-flex flex-column gap-3";
                    cleanElement.innerHTML = modalBody.innerHTML;

                    // 3. Замінюємо в DOM
                    targetElement.parentNode.replaceChild(cleanElement, targetElement);

                    // Оскільки це повна секція поста на всю сторінку, вона тепер просто відображає контент 
                    // і не реагує на кліки для вибору іншого поста.
                }
            });
    }
}

function applySectionType(plugBlock, type) {
    plugBlock.className = "";

    if (type === "story") {
        plugBlock.classList.add("portfolio-story-section");
        plugBlock.innerHTML = `
            <h2 class="plug-card" contenteditable="true"></h2>
            <p class="plug-card" contenteditable="true"></p>
            <button class="btn-delete" aria-label="Видалити розповідь"><span class="icon-delete"></span></button>
        `;
    }
    else if (type === "posts") {
        plugBlock.classList.add("portfolio-posts-section");
        plugBlock.innerHTML = `
            <h2 class="plug-card" contenteditable="true"></h2>
            <div class="row row-cols-1 row-cols-sm-2 row-cols-lg-4 g-4 w-100">
                <div class="col"><div class="plug-card select-post-trigger"><div></div><div></div></div></div>
                <div class="col"><div class="plug-card select-post-trigger"><div></div><div></div></div></div>
                <div class="col"><div class="plug-card select-post-trigger"><div></div><div></div></div></div>
                <div class="col"><div class="plug-card select-post-trigger"><div></div><div></div></div></div> 
            </div>
            <button class="btn-delete" aria-label="Видалити список робіт"><span class="icon-delete"></span></button>
        `;

        plugBlock.querySelectorAll(".select-post-trigger").forEach(elem => {
            elem.addEventListener("click", (e) => {
                e.stopPropagation();
                openChoicePostModal(elem, "posts");
            });
        });
    }
    else if (type === "post") {
        plugBlock.classList.add("portfolio-post-section");
        plugBlock.innerHTML = `
            <div class="plug-card select-single-post-trigger"></div>
            <button class="btn-delete" aria-label="Видалити пост"><span class="icon-delete"></span></button>
        `;

        const trigger = plugBlock.querySelector(".select-single-post-trigger");
        trigger.addEventListener("click", (e) => {
            e.stopPropagation();
            openChoicePostModal(trigger, "post"); // Передаємо trigger як targetElement
        });
    }

    plugBlock.querySelectorAll('[contenteditable="true"]').forEach(el => {
        el.addEventListener("focus", function () {
            this.classList.remove("plug-card");
        });
        el.addEventListener("blur", function () {
            if (this.textContent.trim() === "") {
                this.classList.add("plug-card");
            }
        });
    });

    const btnDelete = plugBlock.querySelector(".btn-delete");
    btnDelete.addEventListener("click", (e) => {
        e.stopPropagation();
        plugBlock.remove();
        if (!portfolioContainer.querySelector(".plug-add-section") && portfolioContainer.querySelectorAll("section").length < 10) {
            addPlugBlock("plug-add-section");
        }
    });

    if (plugBlock === portfolioContainer.lastElementChild && portfolioContainer.querySelectorAll("section").length < 10) {
        addPlugBlock("plug-add-section");
    }
}

function addPlugBlock(plugName) {
    const plugBlock = document.createElement("section");
    plugBlock.classList.add(plugName);
    portfolioContainer.appendChild(plugBlock);

    plugBlock.addEventListener("click", () => {
        createPortfolioSection(plugBlock, plugName);
    });
}

// =========================================================================
// ІНІЦІАЛІЗАЦІЯ ТА КЕРУВАННЯ КОНСТРУКТОРОМ ПОРТФОЛІО
// =========================================================================
// =========================================================================
// ХЕЛПЕРИ ДЛЯ МОДАЛЬНИХ ВІКОН БУТСТРАП (ЗАМІНА ALERT ТА CONFIRM)
// =========================================================================

// Функція для показу сповіщень (успіх, помилка, копіювання посилання)
function showPortfolioAlert(title, message, onCloseCallback = null) {
    const modalEl = document.getElementById("portfolioAlertModal");
    if (!modalEl) return;

    document.getElementById("alertModalLabel").textContent = title;
    document.getElementById("alertModalBody").textContent = message;

    const bsModal = new bootstrap.Modal(modalEl);
    bsModal.show();

    if (onCloseCallback) {
        modalEl.addEventListener('hidden.bs.modal', function handler() {
            onCloseCallback();
            modalEl.removeEventListener('hidden.bs.modal', handler);
        });
    }
}

// Функція для підтвердження дій (очищення конструктора)
function showPortfolioConfirm(title, message, actionText, onConfirmCallback) {
    const modalEl = document.getElementById("portfolioConfirmModal");
    if (!modalEl) return;

    document.getElementById("confirmModalLabel").textContent = title;
    document.getElementById("confirmModalBody").textContent = message;

    const actionBtn = document.getElementById("btnPortfolioConfirmAction");
    actionBtn.textContent = actionText;

    const bsModal = new bootstrap.Modal(modalEl);

    // Перестворюємо кнопку, щоб очистити старі обробники подій від попередніх викликів
    const newActionBtn = actionBtn.cloneNode(true);
    actionBtn.parentNode.replaceChild(newActionBtn, actionBtn);

    newActionBtn.addEventListener("click", () => {
        bsModal.hide();
        onConfirmCallback();
    });

    bsModal.show();
}

// =========================================================================
// ІНІЦІАЛІЗАЦІЯ ТА КЕРУВАННЯ КОНСТРУКТОРОМ ПОРТФОЛІО
// =========================================================================

if (portfolioContainer) {

    // --- ДЕЛЕГУВАННЯ ПОДІЙ ДЛЯ ЗБЕРЕЖЕНОГО В БД ПОРТФОЛІО ---
    portfolioContainer.addEventListener("click", (e) => {
        // СТРОГИЙ ЗАХИСТ: Цей обробник активний ТІЛЬКИ в конструкторі користувача "Моє портфоліо"
        if (!document.getElementById("btnSavePortfolio")) return;

        const card = e.target.closest(".portfolio-post-card-preview");
        if (card) {
            const postId = card.getAttribute("data-post-id");
            if (postId) {
                e.stopPropagation();
                const newUrl = new URL(window.location.href);
                newUrl.searchParams.set('post', postId);
                window.history.pushState({}, '', newUrl);
                openGlobalPostModal(postId);
            }
        }
    });

    // --- ПЕРЕВІРКА НАЯВНОСТІ ПОРТФОЛІО ПРИ ЗАВАНТАЖЕННІ СТОРІНКИ ---
    fetch('/MyPortfolio/GetPortfolio')
        .then(res => res.json())
        .then(data => {
            const btnCopy = document.getElementById("btnCopyPortfolioLink");

            if (data.exists) {
                portfolioContainer.innerHTML = data.content;

                if (btnCopy) {
                    btnCopy.classList.remove("d-none");
                    btnCopy.style.display = "inline-block";
                    btnCopy.setAttribute("data-portfolio-id", data.id);
                }
            } else {
                // Якщо портфоліо немає — запускаємо порожній конструктор, кнопка залишається прихованою
                initConstructor();
                if (btnCopy) {
                    btnCopy.classList.add("d-none");
                    btnCopy.style.display = "none";
                }
            }
        })
        .catch(err => {
            console.error("Помилка завантаження портфоліо:", err);
            initConstructor();
        });

    // Ініціалізація модалки вибору секцій
    if (!document.getElementById("sectionSelectionModal")) {
        document.body.insertAdjacentHTML('beforeend', modalHTML);
    }
    sectionModal = document.getElementById("sectionSelectionModal");
    bsSectionModal = new bootstrap.Modal(sectionModal);

    sectionModal.querySelectorAll(".list-group-item").forEach(option => {
        option.addEventListener("click", () => {
            const choice = option.getAttribute("data-select");
            if (currentActivePlug) {
                isChoiceMade = true;
                applySectionType(currentActivePlug, choice);
            }
            bsSectionModal.hide();
        });
    });
}

function initConstructor() {
    portfolioContainer.innerHTML = "";
    addPlugBlock("plug-add-preamble");
}

// --- ЛОГІКА КНОПКИ "ЗБЕРЕГТИ ЗМІНИ" ---
const btnSavePortfolio = document.getElementById("btnSavePortfolio");
if (btnSavePortfolio) {
    btnSavePortfolio.addEventListener("click", () => {
        const clone = portfolioContainer.cloneNode(true);

        clone.querySelectorAll(".btn-delete, .plug-add-section, .plug-add-preamble, .select-single-post-trigger, .btn-camera").forEach(el => el.remove());

        clone.querySelectorAll(".select-post-trigger").forEach(el => {
            const col = el.closest(".col");
            if (col) col.remove();
            else el.remove();
        });

        const sections = clone.querySelectorAll(".portfolio-section, .section-block, fieldset, section");
        sections.forEach(section => {
            const headingEl = section.querySelector(".section-title, h1, h2, h3, h4, h5, h6");
            const headingText = headingEl ? headingEl.textContent.trim() : "";
            const hasRealPosts = section.querySelectorAll(".portfolio-post-card-preview").length > 0;

            let bodyText = section.textContent.trim();
            if (headingText) {
                bodyText = bodyText.replace(headingEl.textContent, "").trim();
            }

            if (!headingText || (!hasRealPosts && !bodyText)) {
                section.remove();
            }
        });

        clone.querySelectorAll("[contenteditable]").forEach(el => {
            el.removeAttribute("contenteditable");
            el.classList.remove("plug-card");
        });

        const cleanedHtml = clone.innerHTML.trim();

        if (!cleanedHtml || cleanedHtml === "") {
            showPortfolioAlert("Увага", "Неможливо зберегти порожнє портфоліо! Додайте хоча б одну повністю заповнену секцію (із заголовком та вмістом).");
            return;
        }

        const formData = new FormData();
        formData.append("content", cleanedHtml);

        fetch('/MyPortfolio/SavePortfolio', {
            method: 'POST',
            body: formData
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    showPortfolioAlert("Успіх", "Портфоліо успішно збережено!", () => {
                        window.location.reload();
                    });
                } else {
                    showPortfolioAlert("Помилка", "Помилка при збереженні портфоліо.");
                }
            })
            .catch(err => {
                console.error("Помилка відправки:", err);
                showPortfolioAlert("Помилка", "Сталася помилка з'єднання з сервером.");
            });
    });
}

// --- ЛОГІКА КНОПКИ "ОЧИСТИТИ ВСЕ" ---
const btnClearPortfolio = document.getElementById("btnClearPortfolio");
if (btnClearPortfolio) {
    btnClearPortfolio.addEventListener("click", () => {
        showPortfolioConfirm(
            "Очистити портфоліо",
            "Ви впевнені у своїх діях? Усі незбережені дані або поточний вигляд конструктора буде скинуто.",
            "Очистити",
            () => {
                initConstructor();
            }
        );
    });
}

// --- ЛОГІКА КНОПКИ "СКОПІЮВАТИ ПОСИЛАННЯ" (btnSharePortfolio) ---
const btnCopyPortfolioLink = document.getElementById("btnCopyPortfolioLink");
if (btnCopyPortfolioLink) {
    btnCopyPortfolioLink.addEventListener("click", () => {
        const portfolioId = btnCopyPortfolioLink.getAttribute("data-portfolio-id");
        if (!portfolioId || portfolioId === "undefined") {
            showPortfolioAlert("Увага", "Не вдалося знайти ID вашого портфоліо. Спробуйте перезберегти сторінку.");
            return;
        }

        // Формуємо красиве посилання: домен/Portfolio/User?id=значення
        const domain = window.location.origin;
        const shareUrl = `${domain}/Portfolio/User?id=${portfolioId}`;

        // Копіювання в буфер обміну
        navigator.clipboard.writeText(shareUrl)
            .then(() => {
                showPortfolioAlert("Успіх", "Посилання успішно скопійовано в буфер обміну!");
            })
            .catch(err => {
                console.error("Не вдалося скопіювати:", err);
                showPortfolioAlert("Увага", `Не вдалося скопіювати автоматично. Ось ваше посилання:\n${shareUrl}`);
            });
    });
}

// Функція для відображення повноцінного модального вікна за ID
function openGlobalPostModal(postId) {
    let container = document.getElementById("globalPostModalContainer");
    if (!container) {
        container = document.createElement("div");
        container.id = "globalPostModalContainer";
        document.body.appendChild(container);
    }

    fetch(`/MyPortfolio/GetPostModalPartial?id=${postId}`)
        .then(res => res.text())
        .then(html => {
            container.innerHTML = html;
            const modalElem = container.querySelector(".modal");
            if (modalElem) {
                const bsPostModal = new bootstrap.Modal(modalElem);
                bsPostModal.show();
                modalElem.addEventListener("hidden.bs.modal", () => {
                    const newUrl = new URL(window.location.href);
                    newUrl.searchParams.delete('post');
                    window.history.pushState({}, '', newUrl);
                    container.innerHTML = "";
                });
            }
        });
}

//---
document.addEventListener("DOMContentLoaded", () => {
    const mainFilterForm = document.getElementById("mainFilterForm");
    const resetFiltersBtn = document.getElementById("resetFiltersBtn");

    // =========================================================================
    // Логіка керування відображенням та дією кнопки "Скинути фільтри"
    // =========================================================================
    if (mainFilterForm && resetFiltersBtn) {

        // Функція перевірки: чи вибрано хоч якийсь фільтр чи пошук, окрім дефолтного
        const checkFilterStates = () => {
            const checkedCheckboxes = mainFilterForm.querySelectorAll("input[type='checkbox']:checked").length;
            const timeAllRadio = document.getElementById("timeAll");
            const isTimeActive = timeAllRadio ? !timeAllRadio.checked : false;
            const searchInput = mainFilterForm.querySelector("input[name='searchString']");
            const hasSearch = searchInput ? searchInput.value.trim() !== "" : false;

            if (checkedCheckboxes > 0 || isTimeActive || hasSearch) {
                resetFiltersBtn.style.display = "block";
            } else {
                resetFiltersBtn.style.display = "none";
            }
        };

        // Слухаємо зміни форми для динамічного показу кнопки "Скинути"
        mainFilterForm.addEventListener("change", checkFilterStates);

        // Перевіряємо стан одразу при першому завантаженні
        checkFilterStates();

        // Поведінка при кліку на "Скинути фільтри"
        resetFiltersBtn.addEventListener("click", () => {
            // Очищуємо всі чекбокси (Спеціальність, Технології)
            mainFilterForm.querySelectorAll("input[type='checkbox']").forEach(cb => cb.checked = false);

            // Скидаємо текстовий пошук
            const searchInput = mainFilterForm.querySelector("input[name='searchString']");
            if (searchInput) searchInput.value = '';

            // Скидаємо фільтр часу на "За весь час"
            const timeAllRadio = document.getElementById("timeAll");
            if (timeAllRadio) timeAllRadio.checked = true;

            // Зберігаємо поточне сортування, щоб не скидати його
            const sortByRadio = mainFilterForm.querySelector("input[name='sortBy']:checked");
            const currentSort = sortByRadio ? sortByRadio.value : "recommended";

            // Перенаправляємо на чистий URL, звільняючи від параметрів фільтрації
            window.location.href = window.location.pathname + "?sortBy=" + encodeURIComponent(currentSort);
        });
    }

    // =========================================================================
    // Клік на картку-портфоліо для перенаправлення на сторінку User (?id=id)
    // =========================================================================
    document.addEventListener("click", (e) => {
        const portfolioCard = e.target.closest(".portfolio-card");
        if (portfolioCard && portfolioCard.dataset.portfolioId) {
            window.location.href = `/Portfolio/User?id=${portfolioCard.dataset.portfolioId}`;
        }
    });

    // =========================================================================
    // Динамічне відкриття модального вікна посту без перезавантаження сторінки
    // =========================================================================
    const portfolioContainer = document.querySelector(".portfolio-container");
    if (portfolioContainer) {
        portfolioContainer.addEventListener("click", function (e) {
            // СТРОГИЙ ЗАХИСТ: Якщо це конструктор "Моє портфоліо", повністю ІГНОРУЄМО цей обробник публічного AJAX
            if (document.getElementById("btnSavePortfolio")) return;

            const postElement = e.target.closest('[data-post-id]') || e.target.closest('.post-card');
            if (postElement) {
                let postId = postElement.getAttribute('data-post-id');
                if (!postId) {
                    const targetAttr = postElement.getAttribute('data-bs-target') || postElement.getAttribute('id');
                    if (targetAttr && targetAttr.includes('-')) {
                        postId = targetAttr.split('-').pop();
                    }
                }

                if (postId && !isNaN(postId)) {
                    e.preventDefault();

                    const urlParams = new URLSearchParams(window.location.search);
                    const portfolioId = urlParams.get('id');

                    // AJAX завантаження часткового представлення модалки з сервера
                    fetch(`/Portfolio/GetPostDetails?postId=${postId}`)
                        .then(response => {
                            if (!response.ok) throw new Error("Помилка при завантаженні даних поста");
                            return response.text();
                        })
                        .then(html => {
                            // Якщо така модалка вже була згенерована раніше — видаляємо її
                            const existingModal = document.getElementById(`postModal-${postId}`);
                            if (existingModal) existingModal.remove();

                            // Вбудовуємо нову розмітку модалки в кінець body
                            document.body.insertAdjacentHTML('beforeend', html);

                            const modalEl = document.getElementById(`postModal-${postId}`);
                            if (modalEl) {
                                const bootstrapModal = new bootstrap.Modal(modalEl);
                                bootstrapModal.show();

                                // Міняємо URL-адресу без перезавантаження
                                const newUrl = `${window.location.pathname}?id=${portfolioId}&post=${postId}`;
                                history.pushState({ postId: postId }, '', newUrl);

                                // Повертаємо чистий URL (?id=id) після закриття вікна
                                modalEl.addEventListener('hidden.bs.modal', function () {
                                    const baseUrl = `${window.location.pathname}?id=${portfolioId}`;
                                    history.pushState(null, '', baseUrl);
                                    modalEl.remove(); // Видаляємо з DOM для чистоти структури
                                });
                            }
                        })
                        .catch(err => console.error(err));
                }
            }
        });
    }

    // =========================================================================
    // Перевірка прямого посилання на пост (?id=id&post=id) під час завантаження сторінки
    // =========================================================================
    const directPostTrigger = document.getElementById("directPostTrigger");
    if (directPostTrigger) {
        const postId = directPostTrigger.dataset.postId;
        const modalEl = document.getElementById(`postModal-${postId}`);
        if (modalEl) {
            const bootstrapModal = new bootstrap.Modal(modalEl);
            bootstrapModal.show();

            modalEl.addEventListener('hidden.bs.modal', function () {
                const urlParams = new URLSearchParams(window.location.search);
                const portfolioId = urlParams.get('id');
                const baseUrl = `${window.location.pathname}?id=${portfolioId}`;
                history.pushState(null, '', baseUrl);
            });
        }
    }
});

//-----

// Перевірка рядка адреси (URL) під час завантаження сторінки
/*document.addEventListener("DOMContentLoaded", () => {
    const urlParams = new URLSearchParams(window.location.search);
    const postIdFromUrl = urlParams.get('post');

    // Якщо в адресі знайдено ?post=число, автоматично викликаємо вікно
    if (postIdFromUrl) {
        openGlobalPostModal(postIdFromUrl);
    }
});*/

// =========================================================================
// 1. Обробка навігації в лівому меню налаштувань
// =========================================================================
const navLinks = document.querySelectorAll('#navbar-settings-menu .nav-link');
navLinks.forEach(link => {
    link.addEventListener('click', function (e) {
        e.preventDefault();

        const url = this.getAttribute('data-url');
        if (!url) return;

        navLinks.forEach(item => {
            item.classList.remove("active");
        });
        link.classList.add("active");

        fetch(url)
            .then(response => response.text())
            .then(html => {
                const contentContainer = document.getElementById('settingsContent');
                if (contentContainer) {
                    contentContainer.innerHTML = html;
                }
            })
            .catch(error => console.error('Помилка завантаження налаштувань:', error));
    });
});

// =========================================================================
// 2. ЄДИНИЙ універсальний обробник для ВСІХ форм у налаштуваннях
// (Збереження даних, Зміна паролю, Видалення акаунта)
// =========================================================================
const settingsContent = document.getElementById('settingsContent');

// Робимо залізобетонну перевірку, щоб JS не падав на інших сторінках сайту
if (settingsContent) {
    settingsContent.addEventListener('submit', function (e) {
        const form = e.target;

        if (form && form.tagName === 'FORM') {
            // Безпечно дістаємо action форми
            const url = form.action || form.getAttribute('action');
            if (!url) return;

            // Якщо це стандартна форма виходу з акаунта, даємо їй спрацювати звичайно
            if (url.includes('Logout')) {
                return;
            }

            e.preventDefault(); // Зупиняємо стандартне перезавантаження сторінки

            const formData = new FormData(form);

            fetch(url, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(response => {
                    const contentType = response.headers.get("content-type");

                    // ВАРІАНТ А: Сервер повернув JSON (це наш випадок із видаленням акаунта)
                    if (contentType && contentType.indexOf("application/json") !== -1) {
                        return response.json().then(data => {
                            if (data.redirectUrl) {
                                // Якщо відкрита модалка видалення, закриваємо її через Bootstrap API
                                const modalEl = document.getElementById('deleteAccountModal');
                                if (modalEl) {
                                    const modalInstance = bootstrap.Modal.getInstance(modalEl);
                                    modalInstance?.hide();

                                    // Зачищаємо за собою темний фон модалки, який іноді зависає при редіректах
                                    document.querySelector('.modal-backdrop')?.remove();
                                    document.body.style.overflow = '';
                                }
                                // Робимо редірект на Головну
                                window.location.href = data.redirectUrl;
                            }
                        });
                    }
                    // ВАРІАНТ Б: Сервер повернув HTML-частку (для збереження профілю чи зміни паролю)
                    else {
                        return response.text().then(html => {
                            document.getElementById('settingsContent').innerHTML = html;
                        });
                    }
                })
                .catch(error => console.error('Помилка обробки форми:', error));
        }
    });
}

// =========================================================================
// 3. Обробка динамічної зміни аватара (працює через делегування подій на рівні document)
// =========================================================================
document.addEventListener("change", (e) => {
    if (e.target && e.target.id === "settingsAvatarInput") {
        const input = e.target;
        if (input.files && input.files[0]) {
            const avatarResult = document.getElementById("avatarResult");
            if (avatarResult) {
                const objectUrl = URL.createObjectURL(input.files[0]);
                avatarResult.style.backgroundImage = `url(${objectUrl})`;
                // Очищення пам'яті (опціонально для <div>)
                avatarResult.onload = () => URL.revokeObjectURL(objectUrl);
            }
        }
    }
});
// =========================================================================
// 4. Керування видимістю портфоліо (безшовне AJAX оновлення БД)
// =========================================================================
document.addEventListener("change", function (e) {
    if (e.target && e.target.id === "portfolioHideToggle") {
        const checkbox = e.target;
        const url = checkbox.getAttribute("data-url");

        // Динамічно знаходимо AntiForgeryToken захисту з DOM
        const tokenInput = document.querySelector('#deletePortfolioForm input[name="__RequestVerificationToken"]')
            || document.querySelector('.form-check input[name="__RequestVerificationToken"]');
        const token = tokenInput ? tokenInput.value : '';

        const formData = new FormData();
        formData.append('isVisible', checkbox.checked);
        formData.append('__RequestVerificationToken', token);

        fetch(url, {
            method: 'POST',
            body: formData
        })
            .then(response => response.text())
            .then(html => {
                // Безшовно замінюємо вміст робочої вкладки налаштувань
                const contentBlock = document.getElementById('settingsContent');
                if (contentBlock) {
                    contentBlock.innerHTML = html;
                }
            })
            .catch(error => console.error('Помилка оновлення статусу видимості:', error));
    }
});