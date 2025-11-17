document.addEventListener('DOMContentLoaded', function () {

    const taskCheckboxes = document.querySelectorAll('.task-completed-checkbox');

    taskCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', async function (e) {
            const taskId = this.getAttribute('data-task-id');
            const taskRow = this.closest('.task-item');
            const taskTitle = taskRow.querySelector('.task-title');
            const originalCheckedState = !this.checked;

            try {
                // Отправка AJAX запроса
                const response = await fetch(`/TodoTask/Toggle/${taskId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    }
                });

                if (response.ok) {
                    const result = await response.json();


                    if (result.isCompleted) {
                        taskTitle.classList.add('text-decoration-line-through', 'text-muted');
                        taskRow.classList.add('completed-task');


                        taskRow.style.opacity = '0.6';
                        setTimeout(() => {
                            taskRow.style.opacity = '1';
                        }, 300);
                    } else {
                        taskTitle.classList.remove('text-decoration-line-through', 'text-muted');
                        taskRow.classList.remove('completed-task');
                    }

                    updateTaskCounters();

                    showToast('Task updated successfully', 'success');
                } else {
                    throw new Error('Failed to update task');
                }

            } catch (error) {
                console.error('Error toggling task:', error);
                this.checked = originalCheckedState;

                alert('Failed to update task. Please try again.');
            }
        });
    });
});

function updateTaskCounters() {
    const allTasks = document.querySelectorAll('.task-item');
    const completedTasks = document.querySelectorAll('.task-item.completed-task');

    const totalCount = allTasks.length;
    const completedCount = completedTasks.length;
    const pendingCount = totalCount - completedCount;
    const percentage = totalCount > 0 ? Math.round((completedCount / totalCount) * 100) : 0;

    // Обновить элементы на странице
    const totalElement = document.getElementById('total-tasks-count');
    const completedElement = document.getElementById('completed-tasks-count');
    const pendingElement = document.getElementById('pending-tasks-count');
    const progressBar = document.getElementById('completion-progress-bar');

    if (totalElement) totalElement.textContent = totalCount;
    if (completedElement) completedElement.textContent = completedCount;
    if (pendingElement) pendingElement.textContent = pendingCount;

    if (progressBar) {
        progressBar.style.width = percentage + '%';
        progressBar.setAttribute('aria-valuenow', percentage);
        progressBar.textContent = percentage + '%';

        // Изменить цвет прогресс-бара
        progressBar.classList.remove('bg-danger', 'bg-warning', 'bg-success');
        if (percentage < 30) {
            progressBar.classList.add('bg-danger');
        } else if (percentage < 70) {
            progressBar.classList.add('bg-warning');
        } else {
            progressBar.classList.add('bg-success');
        }
    }
}

// ========================================
// 2. DELETE TASK
// ========================================

const deleteTaskButtons = document.querySelectorAll('.delete-task-btn');

deleteTaskButtons.forEach(button => {
    button.addEventListener('click', function (e) {
        e.preventDefault();

        const taskId = this.getAttribute('data-task-id');
        const taskTitle = this.getAttribute('data-task-title');
        const taskRow = this.closest('.task-item');

        showDeleteConfirmModal(taskId, taskTitle, taskRow);
    });
});

function showDeleteConfirmModal(taskId, taskTitle, taskRow) {
    let modal = document.getElementById('deleteTaskModal');

    if (!modal) {
        modal = createDeleteModal();
        document.body.appendChild(modal);
    }
    const modalTitle = modal.querySelector('#deleteTaskModalLabel');
    const modalBody = modal.querySelector('.modal-body');
    const confirmButton = modal.querySelector('#confirmDeleteTaskBtn');

    modalTitle.textContent = 'Delete Task';
    modalBody.innerHTML = `<p>Are you sure you want to delete the task:</p><p class="fw-bold">"${taskTitle}"</p><p class="text-danger">This action cannot be undone.</p>`;

    const newConfirmButton = confirmButton.cloneNode(true);
    confirmButton.parentNode.replaceChild(newConfirmButton, confirmButton);

    newConfirmButton.addEventListener('click', async function () {
        await deleteTask(taskId, taskRow);

        const bsModal = bootstrap.Modal.getInstance(modal);
        bsModal.hide();
    });

    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();
}

function createDeleteModal() {
    const modalHTML = `
        <div class="modal fade" id="deleteTaskModal" tabindex="-1" aria-labelledby="deleteTaskModalLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="deleteTaskModalLabel">Delete Task</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <!-- Content will be inserted here -->
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <button type="button" class="btn btn-danger" id="confirmDeleteTaskBtn">Delete</button>
                    </div>
                </div>
            </div>
        </div>
    `;

    const div = document.createElement('div');
    div.innerHTML = modalHTML;
    return div.firstElementChild;
}

async function deleteTask(taskId, taskRow) {
    try {
        const response = await fetch(`/TodoTask/Delete/${taskId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        });

        if (response.ok) {
            taskRow.style.transition = 'all 0.3s ease';
            taskRow.style.opacity = '0';
            taskRow.style.transform = 'translateX(-20px)';

            setTimeout(() => {
                taskRow.remove();

                updateTaskCounters();

                checkEmptyTaskList();

                showToast('Task deleted successfully', 'success');
            }, 300);

        } else {
            throw new Error('Failed to delete task');
        }

    } catch (error) {
        console.error('Error deleting task:', error);
        alert('Failed to delete task. Please try again.');
    }
}

function checkEmptyTaskList() {
    const taskList = document.getElementById('tasks-list');
    const tasks = taskList.querySelectorAll('.task-item');

    if (tasks.length === 0) {
        // Показать empty state
        taskList.innerHTML = `
            <div class="text-center py-5 text-muted" id="empty-state">
                <i class="bi bi-inbox" style="font-size: 3rem;"></i>
                <p class="mt-3">No tasks yet. Add your first task below!</p>
            </div>
        `;
    }
}

// ========================================
// 3. ADD TASK (через AJAX)
// ========================================

const addTaskForm = document.getElementById('add-task-form');

if (addTaskForm) {
    addTaskForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const formData = new FormData(this);

        const taskData = {
            title: formData.get('Title'),
            description: formData.get('Description'),
            dueDate: formData.get('DueDate'),
            priority: formData.get('Priority')
        };

        if (!taskData.title || taskData.title.trim() === '') {
            alert('Task title is required');
            return;
        }

        try {
            const listId = document.getElementById('todolist-id').value;

            const response = await fetch(`/TodoTask/Create/${listId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': formData.get('__RequestVerificationToken')
                },
                body: JSON.stringify(taskData)
            });

            if (response.ok) {
                const result = await response.json();

                const emptyState = document.getElementById('empty-state');
                if (emptyState) {
                    emptyState.remove();
                }

                const newTaskHtml = createTaskHtml(result);

                const tasksList = document.getElementById('tasks-list');
                tasksList.insertAdjacentHTML('afterbegin', newTaskHtml);

                const newTask = tasksList.firstElementChild;
                newTask.style.opacity = '0';
                newTask.style.transform = 'translateY(-10px)';

                setTimeout(() => {
                    newTask.style.transition = 'all 0.3s ease';
                    newTask.style.opacity = '1';
                    newTask.style.transform = 'translateY(0)';
                }, 10);

                addTaskForm.reset();

                updateTaskCounters();

                attachTaskEventHandlers(newTask);


                showToast('Task added successfully', 'success');

            } else {
                const errorData = await response.json();
                alert('Failed to add task: ' + (errorData.message || 'Unknown error'));
            }

        } catch (error) {
            console.error('Error adding task:', error);
            alert('Failed to add task. Please try again.');
        }
    });
}

function createTaskHtml(task) {
    const completedClass = task.isCompleted ? 'completed-task' : '';
    const titleClass = task.isCompleted ? 'text-decoration-line-through text-muted' : '';
    const checked = task.isCompleted ? 'checked' : '';
    const overdueClass = task.isOverdue ? 'text-danger' : '';
    const dueDateText = task.formattedDueDate ? `<small class="${overdueClass}"><i class="bi bi-calendar"></i> ${task.formattedDueDate}</small>` : '';

    return `
        <div class="task-item card mb-2 ${completedClass}" data-task-id="${task.id}">
            <div class="card-body d-flex align-items-center">
                <input type="checkbox" class="form-check-input task-completed-checkbox me-3" 
                       data-task-id="${task.id}" ${checked}>
                <div class="flex-grow-1">
                    <h6 class="task-title mb-1 ${titleClass}">${escapeHtml(task.title)}</h6>
                    ${task.description ? `<p class="mb-0 text-muted small">${escapeHtml(task.description)}</p>` : ''}
                    ${dueDateText}
                </div>
                <div class="btn-group btn-group-sm">
                    <button class="btn btn-outline-secondary edit-task-btn" data-task-id="${task.id}">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-outline-danger delete-task-btn" 
                            data-task-id="${task.id}" 
                            data-task-title="${escapeHtml(task.title)}">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </div>
        </div>
    `;
}

function attachTaskEventHandlers(taskElement) {
    const checkbox = taskElement.querySelector('.task-completed-checkbox');
    if (checkbox) {
        checkbox.addEventListener('change', async function (e) {
        });
    }

    const deleteBtn = taskElement.querySelector('.delete-task-btn');
    if (deleteBtn) {
        deleteBtn.addEventListener('click', function (e) {
            e.preventDefault();
            const taskId = this.getAttribute('data-task-id');
            const taskTitle = this.getAttribute('data-task-title');
            const taskRow = this.closest('.task-item');
            showDeleteConfirmModal(taskId, taskTitle, taskRow);
        });
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}