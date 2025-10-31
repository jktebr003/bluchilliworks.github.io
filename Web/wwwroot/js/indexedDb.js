window.indexedDbManager = {
    db: null,
    init: function () {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open("SafetyAppDatabase", 1);

            request.onerror = function () {
                reject("Database failed to open");
            };

            request.onsuccess = function () {
                indexedDbManager.db = request.result;
                resolve();
            };

            request.onupgradeneeded = function (e) {
                const db = e.target.result;
                db.createObjectStore("notes", { keyPath: "id" });
            };
        });
    },

    addNote: function (note) {
        return new Promise((resolve, reject) => {
            const tx = indexedDbManager.db.transaction("notes", "readwrite");
            const store = tx.objectStore("notes");
            store.put(note);
            tx.oncomplete = () => resolve();
            tx.onerror = () => reject(tx.error);
        });
    },

    getAllNotes: function () {
        return new Promise((resolve, reject) => {
            const tx = indexedDbManager.db.transaction("notes", "readonly");
            const store = tx.objectStore("notes");
            const request = store.getAll();

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    // Get unsynced notes
    getUnsyncedNotes: function () {
        return new Promise((resolve, reject) => {
            const tx = indexedDbManager.db.transaction("notes", "readonly");
            const store = tx.objectStore("notes");
            const request = store.getAll();

            request.onsuccess = () => {
                const unsynced = request.result.filter(n => !n.synced);
                resolve(unsynced);
            };
            request.onerror = () => reject(request.error);
        });
    },

    // Mark note as synced
    markAsSynced: function (id) {
        return new Promise((resolve, reject) => {
            const tx = indexedDbManager.db.transaction("notes", "readwrite");
            const store = tx.objectStore("notes");
            const getRequest = store.get(id);

            getRequest.onsuccess = () => {
                const note = getRequest.result;
                note.synced = true;
                store.put(note);
                tx.oncomplete = () => resolve();
            };

            getRequest.onerror = () => reject(getRequest.error);
        });
    },

    bulkUpsertNotes: function (notes) {
        return new Promise((resolve, reject) => {
            const tx = indexedDbManager.db.transaction("notes", "readwrite");
            const store = tx.objectStore("notes");

            const processNext = (index) => {
                if (index >= notes.length) {
                    resolve();
                    return;
                }

                const incomingNote = notes[index];
                const getRequest = store.get(incomingNote.id);

                getRequest.onsuccess = () => {
                    const existing = getRequest.result;

                    if (!existing || new Date(incomingNote.lastModified) > new Date(existing.lastModified)) {
                        store.put({ ...incomingNote, synced: true });
                    }

                    processNext(index + 1);
                };

                getRequest.onerror = () => reject(getRequest.error);
            };

            processNext(0);
        });
    },

    // Generic methods
    initialize: function (dbName, version, objectStores) {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(dbName, version);

            request.onerror = function () {
                reject("Database failed to open");
            };

            request.onsuccess = function () {
                indexedDbManager.db = request.result;
                resolve();
            };

            request.onupgradeneeded = function (e) {
                const db = e.target.result;

                objectStores.forEach(store => {
                    if (!db.objectStoreNames.contains(store.name)) {
                        const objectStore = db.createObjectStore(store.name, { keyPath: store.keyPath, autoIncrement: store.autoIncrement });

                        if (store.indexes) {
                            store.indexes.forEach(index => {
                                objectStore.createIndex(index.name, index.keyPath, { unique: index.unique });
                            });
                        }
                    }
                });
            };
        });
    },

    add: function (storeName, entity) {
        return new Promise((resolve, reject) => {
            if (!indexedDbManager.db) {
                reject("IndexedDB is not initialized. Call initialize() first.");
                return;
            }

            // Ensure the synced flag is set to false when adding a new entity
            const entityToAdd = { ...entity, synced: false };

            const tx = indexedDbManager.db.transaction(storeName, "readwrite");
            const store = tx.objectStore(storeName);
            store.put(entityToAdd);
            tx.oncomplete = () => resolve();
            tx.onerror = () => reject(tx.error);
        });
    },

    getAll: function (storeName) {
        return new Promise((resolve, reject) => {
            if (!indexedDbManager.db) {
                reject("IndexedDB is not initialized. Call initialize() first.");
                return;
            }

            const tx = indexedDbManager.db.transaction(storeName, "readonly");
            const store = tx.objectStore(storeName);
            const request = store.getAll();

            request.onsuccess = () => {
                // Filter out items where deleted is true
                const notDeleted = request.result.filter(entity => !entity.deleted);
                resolve(notDeleted);
            };
            request.onerror = () => reject(request.error);
        });
    },

    getById: function (storeName, id) {
        return new Promise((resolve, reject) => {
            if (!indexedDbManager.db) {
                reject("IndexedDB is not initialized. Call initialize() first.");
                return;
            }
            const tx = indexedDbManager.db.transaction(storeName, "readonly");
            const store = tx.objectStore(storeName);
            const request = store.get(id);

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },


    getUnsynced: function (storeName) {
        return new Promise((resolve, reject) => {
            if (!indexedDbManager.db) {
                reject("IndexedDB is not initialized. Call initialize() first.");
                return;
            }

            const tx = indexedDbManager.db.transaction(storeName, "readonly");
            const store = tx.objectStore(storeName);
            const request = store.getAll();

            request.onsuccess = () => {
                const unsynced = request.result.filter(entity => !entity.synced);
                resolve(unsynced);
            };
            request.onerror = () => reject(request.error);
        });
    },

    markAsSynced: function (storeName, id) {
        return new Promise((resolve, reject) => {
            if (!indexedDbManager.db) {
                reject("IndexedDB is not initialized. Call initialize() first.");
                return;
            }

            const tx = indexedDbManager.db.transaction(storeName, "readwrite");
            const store = tx.objectStore(storeName);
            const getRequest = store.get(id);

            getRequest.onsuccess = () => {
                const entity = getRequest.result;
                entity.synced = true;
                store.put(entity);
                tx.oncomplete = () => resolve();
            };

            getRequest.onerror = () => reject(getRequest.error);
        });
    },

    bulkUpsert: function (storeName, entities, forceUpdate = false) {
        return new Promise((resolve, reject) => {
            if (!indexedDbManager.db) {
                reject("IndexedDB is not initialized. Call initialize() first.");
                return;
            }

            const tx = indexedDbManager.db.transaction(storeName, "readwrite");
            const store = tx.objectStore(storeName);

            const processNext = (index) => {
                if (index >= entities.length) {
                    resolve();
                    return;
                }

                const incomingEntity = entities[index];
                const getRequest = store.get(incomingEntity.id);

                getRequest.onsuccess = () => {
                    const existing = getRequest.result;

                    if (!existing || (!existing.synced && existing.updatedDate == null) ||  new Date(incomingEntity.updatedDate) > new Date(existing.updatedDate)) {
                        store.put({ ...incomingEntity, synced: true });
                    }

                    processNext(index + 1);
                };

                getRequest.onerror = () => reject(getRequest.error);
            };

            processNext(0);
        });
    },

    updateById: function (storeName, id, updatedEntity) {
        return new Promise((resolve, reject) => {
            if (!indexedDbManager.db) {
                reject("IndexedDB is not initialized. Call initialize() first.");
                return;
            }
            const tx = indexedDbManager.db.transaction(storeName, "readwrite");
            const store = tx.objectStore(storeName);
            const getRequest = store.get(id);

            getRequest.onsuccess = () => {
                const existing = getRequest.result;
                if (!existing) {
                    reject(`Entity with id ${id} not found in store ${storeName}.`);
                    return;
                }
                // Overwrite the entity (or merge as needed)
                store.put({ ...existing, ...updatedEntity, id: id });
                tx.oncomplete = () => resolve();
                tx.onerror = () => reject(tx.error);
            };
            getRequest.onerror = () => reject(getRequest.error);
        });
    },

    deleteIfNotSynced: function (storeName, id) {
        return new Promise((resolve, reject) => {
            if (!indexedDbManager.db) {
                reject("IndexedDB is not initialized. Call initialize() first.");
                return;
            }
            const tx = indexedDbManager.db.transaction(storeName, "readwrite");
            const store = tx.objectStore(storeName);
            const getRequest = store.get(id);

            getRequest.onsuccess = () => {
                const entity = getRequest.result;
                if (entity && entity.synced === false) {
                    store.delete(id);
                    tx.oncomplete = () => resolve(true);
                    tx.onerror = () => reject(tx.error);
                } else {
                    // Not deleted (either doesn't exist or is synced)
                    resolve(false);
                }
            };
            getRequest.onerror = () => reject(getRequest.error);
        });
    },

    clear: function (storeName) {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open('SafetyAppDatabase');
            request.onsuccess = function (event) {
                const db = event.target.result;
                const transaction = db.transaction([storeName], 'readwrite');
                const objectStore = transaction.objectStore(storeName);
                const clearRequest = objectStore.clear();
                clearRequest.onsuccess = function () { resolve(); };
                clearRequest.onerror = function (e) { reject(e); };
            };
            request.onerror = function (e) { reject(e); };
        });
    }

};
