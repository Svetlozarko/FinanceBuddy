import { Chart } from "@/components/ui/chart"
// Global variables
let expenseChart, incomeChart, savingsChart
let bootstrap // Declare the bootstrap variable

// Initialize the dashboard with server data
function initializeDashboard(serverData) {
    // IMPORT FUNCTIONS
    window.triggerFileInput = (type) => {
        const fileInput = document.getElementById("importFileInput")
        // Set accept attribute based on type
        switch (type) {
            case "csv":
                fileInput.accept = ".csv"
                break
            case "excel":
                fileInput.accept = ".xlsx,.xls"
                break
            case "pdf":
                fileInput.accept = ".pdf"
                break
            default:
                fileInput.accept = ".csv,.xlsx,.xls,.pdf"
        }
        fileInput.click()
    }

    window.handleFileImport = async (input) => {
        const file = input.files[0]
        if (!file) return

        // Show progress modal
        const progressModal = new bootstrap.Modal(document.getElementById("importProgressModal"))
        progressModal.show()

        const formData = new FormData()
        formData.append("file", file)

        try {
            const response = await fetch("/ImportExport/ImportFile", {
                method: "POST",
                body: formData,
            })

            const result = await response.json()

            // Hide progress modal
            progressModal.hide()

            // Show results modal
            showImportResults(result)
        } catch (error) {
            progressModal.hide()
            showImportResults({
                success: false,
                message: "Error uploading file: " + error.message,
            })
        }

        // Clear the input
        input.value = ""
    }

    function showImportResults(result) {
        const resultsModal = new bootstrap.Modal(document.getElementById("importResultsModal"))
        const contentDiv = document.getElementById("importResultsContent")
        let content = ""

        if (result.success) {
            content = `
                <div class="alert alert-success">
                    <i class="fas fa-check-circle me-2"></i>
                    <strong>Import Successful!</strong>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <div class="card bg-light">
                            <div class="card-body text-center">
                                <h5 class="text-success">${result.imported || 0}</h5>
                                <small>Transactions Imported</small>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="card bg-light">
                            <div class="card-body text-center">
                                <h5 class="text-warning">${result.duplicates || 0}</h5>
                                <small>Duplicates Skipped</small>
                            </div>
                        </div>
                    </div>
                </div>
            `
        } else {
            content = `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-circle me-2"></i>
                    <strong>Import Failed</strong>
                </div>
                <p>${result.message}</p>
            `
        }

        if (result.errors && result.errors.length > 0) {
            content += `
                <div class="alert alert-warning mt-3">
                    <h6><i class="fas fa-exclamation-triangle me-2"></i>Warnings:</h6>
                    <ul class="mb-0">
                        ${result.errors.map((error) => `<li>${error}</li>`).join("")}
                    </ul>
                </div>
            `
        }

        contentDiv.innerHTML = content
        resultsModal.show()
    }

    // Submit Income Function
    window.submitIncome = async (event) => {
        event.preventDefault()
        const amountInput = document.getElementById("incomeAmount")
        const errorDiv = document.getElementById("incomeAmountError")

        if (!amountInput || !errorDiv) {
            alert("Required elements not found")
            return
        }

        const amountValue = amountInput.value.trim()

        // Clear previous error
        errorDiv.style.display = "none"
        errorDiv.textContent = ""

        if (!amountValue) {
            errorDiv.textContent = "Amount is required."
            errorDiv.style.display = "block"
            amountInput.focus()
            return
        }

        const amount = Number.parseFloat(amountValue)
        if (isNaN(amount) || amount <= 0) {
            errorDiv.textContent = "Please enter a valid positive amount."
            errorDiv.style.display = "block"
            amountInput.focus()
            return
        }

        const data = { Amount: amount }

        try {
            const response = await fetch("/Incomes/AddIncome", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(data),
            })

            if (!response.ok) {
                let errorMsg = "Unknown error"
                try {
                    const errorData = await response.json()
                    errorMsg = errorData.details || errorData.error || errorMsg
                } catch {
                    // fallback if JSON parsing fails
                }
                alert("Error saving income:\n\n" + errorMsg)
                return
            }

            const result = await response.json()
            if (result.success) {
                const modalElement = document.getElementById("addIncomeModal")
                const modal = bootstrap.Modal.getInstance(modalElement)
                modal.hide()
                document.getElementById("addIncomeForm").reset()
                location.reload() // Refresh to show updated data
            } else {
                alert("Failed to save income.")
            }
        } catch (error) {
            alert("Unexpected error: " + error.message)
        }
    }

    // Submit Savings Function
    window.submitSavings = async (event) => {
        event.preventDefault()
        const targetInput = document.getElementById("targetAmount")
        const currentInput = document.getElementById("currentAmount")
        const targetErrorDiv = document.getElementById("targetAmountError")
        const currentErrorDiv = document.getElementById("currentAmountError")

        // Clear previous errors
        targetErrorDiv.style.display = "none"
        targetErrorDiv.textContent = ""
        currentErrorDiv.style.display = "none"
        currentErrorDiv.textContent = ""

        if (!targetInput || !targetErrorDiv || !currentErrorDiv) {
            alert("Required elements not found")
            return
        }

        const targetValue = targetInput.value.trim()
        const currentValue = currentInput.value.trim()

        if (!targetValue) {
            targetErrorDiv.textContent = "Target amount is required."
            targetErrorDiv.style.display = "block"
            targetInput.focus()
            return
        }

        const targetAmount = Number.parseFloat(targetValue)
        if (isNaN(targetAmount) || targetAmount <= 0) {
            targetErrorDiv.textContent = "Please enter a valid positive target amount."
            targetErrorDiv.style.display = "block"
            targetInput.focus()
            return
        }

        let currentAmount = 0
        if (currentValue) {
            currentAmount = Number.parseFloat(currentValue)
            if (isNaN(currentAmount) || currentAmount < 0) {
                currentErrorDiv.textContent = "Please enter a valid current amount or leave empty."
                currentErrorDiv.style.display = "block"
                currentInput.focus()
                return
            }
        }

        const data = {
            TargetAmount: targetAmount,
            CurrentAmount: currentAmount,
        }

        try {
            const response = await fetch("/Savings/SaveOrUpdateSavings", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(data),
            })

            if (!response.ok) {
                let errorMsg = "Unknown error"
                try {
                    const errorData = await response.json()
                    errorMsg = errorData.details || errorData.error || errorMsg
                } catch {
                    // fallback if JSON parsing fails
                }
                alert("Error saving savings:\n\n" + errorMsg)
                return
            }

            const result = await response.json()
            if (result.success) {
                const modalElement = document.getElementById("addSavingsModal")
                const modal = bootstrap.Modal.getInstance(modalElement)
                modal.hide()
                document.getElementById("addSavingsForm").reset()
                location.reload() // Refresh to show updated data
            } else {
                alert("Failed to save savings.")
            }
        } catch (error) {
            alert("Unexpected error: " + error.message)
        }
    }

    // Submit Expense Function
    window.submitExpense = async (event) => {
        event.preventDefault()
        const amount = Number.parseFloat(document.getElementById("expenseAmount").value)
        const category = document.getElementById("expenseCategory").value
        const date = document.getElementById("expenseDate").value
        const description = document.getElementById("expenseNote").value

        if (!amount || !category || !date) {
            alert("Please fill all required fields.")
            return
        }

        const data = {
            Amount: amount,
            Category: category,
            Date: date,
            Description: description,
        }

        try {
            const response = await fetch("/Expenses/AddExpense", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(data),
            })

            if (!response.ok) {
                const error = await response.json()
                const errorText = error.details ? JSON.stringify(error.details) : error.error || "Unknown error"
                alert("Error saving expense:\n\n" + errorText)
                return
            }

            const result = await response.json()
            if (result.success) {
                const modal = bootstrap.Modal.getInstance(document.getElementById("addExpenseModal"))
                modal.hide()
                document.getElementById("addExpenseForm").reset()
                location.reload() // Refresh to show updated data
            } else {
                alert("Failed to save expense.")
            }
        } catch (error) {
            alert("Unexpected error: " + error.message)
        }
    }

    // Initialize Charts with server data
    initializeCharts(serverData)
}

function initializeCharts(data) {
    // Expense Chart
    const expenseCtx = document.getElementById("expenseChart").getContext("2d")
    expenseChart = new Chart(expenseCtx, {
        type: "doughnut",
        data: {
            labels: data.expenseLabels,
            datasets: [
                {
                    data: data.expenseData,
                    backgroundColor: ["#ef4444", "#3b82f6", "#10b981", "#f59e0b", "#8b5cf6", "#ec4899", "#14b8a6", "#f97316"],
                    borderWidth: 0,
                    hoverOffset: 8,
                },
            ],
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: "60%",
            plugins: {
                legend: {
                    position: "bottom",
                    labels: {
                        padding: 20,
                        usePointStyle: true,
                        font: { size: 12 },
                    },
                },
                tooltip: {
                    backgroundColor: "rgba(0, 0, 0, 0.8)",
                    titleColor: "white",
                    bodyColor: "white",
                    cornerRadius: 8,
                },
            },
        },
    })

    // Income vs Expenses Chart
    const incomeCtx = document.getElementById("incomeChart").getContext("2d")
    incomeChart = new Chart(incomeCtx, {
        type: "bar",
        data: {
            labels: data.incomeVsExpenseLabels,
            datasets: [
                {
                    data: data.incomeVsExpenseData,
                    backgroundColor: ["#ef4444", "#10b981"],
                    borderRadius: 8,
                    borderSkipped: false,
                },
            ],
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: "rgba(0, 0, 0, 0.8)",
                    cornerRadius: 8,
                },
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { font: { size: 12 } },
                },
                y: {
                    grid: { color: "#f3f4f6" },
                    ticks: { display: false },
                },
            },
        },
    })

    // Savings Progress Chart
    const currentAmount = data.currentSavings
    const targetAmount = data.savingsGoal
    const remainingAmount = Math.max(targetAmount - currentAmount, 0)

    const savingsCtx = document.getElementById("savingsChart").getContext("2d")
    savingsChart = new Chart(savingsCtx, {
        type: "doughnut",
        data: {
            labels: ["Current Savings", "Remaining to Goal"],
            datasets: [
                {
                    data: [currentAmount, remainingAmount],
                    backgroundColor: ["#3b82f6", "#e5e7eb"],
                    borderWidth: 0,
                    cutout: "75%",
                },
            ],
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: "rgba(0, 0, 0, 0.8)",
                    cornerRadius: 8,
                },
            },
        },
        plugins: [
            {
                beforeDraw: (chart) => {
                    const width = chart.width,
                        height = chart.height,
                        ctx = chart.ctx
                    ctx.restore()
                    const fontSize = (height / 114).toFixed(2)
                    ctx.font = "bold " + fontSize + "em sans-serif"
                    ctx.textBaseline = "middle"
                    ctx.fillStyle = "#111827"
                    const percentage = targetAmount > 0 ? Math.round((currentAmount / targetAmount) * 100) : 0
                    const text = percentage + "%"
                    const textX = Math.round((width - ctx.measureText(text).width) / 2)
                    const textY = height / 2
                    ctx.fillText(text, textX, textY)
                    ctx.save()
                },
            },
        ],
    })
}

// Initialize when DOM is loaded
document.addEventListener("DOMContentLoaded", () => {
    // Wait for server data to be passed from the Razor page
    if (typeof window.dashboardData !== "undefined") {
        initializeDashboard(window.dashboardData)
    }
})

     const fab = document.getElementById('ai-fab');
        const modal = document.getElementById('aiChatModal');
        const closeBtn = document.getElementById('aiChatClose');
        const chatForm = document.getElementById('aiChatForm');
        const chatInput = document.getElementById('aiChatInput');
        const chatMessages = document.getElementById('aiChatMessages');

        fab.addEventListener('click', () => {
            modal.classList.add('active');
            setTimeout(() => chatInput.focus(), 250);
        });

        closeBtn.addEventListener('click', () => {
            modal.classList.remove('active');
        });

        // Simple local message logic (demo)
        chatForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const msg = chatInput.value.trim();
            if (!msg) return;

            appendUserMessage(msg);
            chatInput.value = '';
            chatMessages.scrollTop = chatMessages.scrollHeight;

            // Simulate bot reply
            setTimeout(() => {
                appendBotMessage("I'm still learning! (Demo)");
                chatMessages.scrollTop = chatMessages.scrollHeight;
            }, 800);
        });

        function appendUserMessage(text) {
            const msg = document.createElement('div');
            msg.className = 'ai-chat-message ai-chat-message-user';
            msg.innerHTML = `
            <div class="ai-chat-message-content">${escapeHtml(text)}</div>
            <div class="ai-chat-message-time">${formatTime()}</div>
          `;
            chatMessages.appendChild(msg);
        }

        function appendBotMessage(text) {
            const msg = document.createElement('div');
            msg.className = 'ai-chat-message ai-chat-message-bot';
            msg.innerHTML = `
            <div class="ai-chat-message-content">${escapeHtml(text)}</div>
            <div class="ai-chat-message-time">${formatTime()}</div>
          `;
            chatMessages.appendChild(msg);
        }

        function escapeHtml(unsafe) {
            return unsafe.replace(/[&<"'>]/g, function (m) {
                return ({
                    '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;'
                })[m];
            });
        }

        function formatTime() {
            const d = new Date();
            let h = d.getHours(), m = d.getMinutes();
            let ampm = h >= 12 ? 'PM' : 'AM';
            h = h % 12; if (h === 0) h = 12;
            return `${h}:${m.toString().padStart(2, '0')} ${ampm}`;
        }
