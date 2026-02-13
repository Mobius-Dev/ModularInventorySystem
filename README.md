# Modular Inventory System

A robust, data-driven inventory solution for Unity, designed with a focus on separation of concerns, memory management, and scalable architecture.

## 📖 Overview

This project demonstrates a production-ready inventory backend and UI frontend. It moves beyond standard tutorials by implementing industry-standard patterns for data persistence, event-driven UI updates, and robust edge-case handling (e.g., partial stack merges, fallback placement).

**Core Design Philosophy:**

* **Separation of Data & View:** The UI (Slot.cs, Tile.cs) is strictly a reflection of the internal data state.

* **Event-Driven Architecture:** UI elements react to data changes via C# Events and Properties, eliminating usage of Update() for UI logic.

* **Input Agnostic:** Logic is decoupled from specific input hardware, utilizing a central Input Manager.

## ✨ Key Features

* **Complex Stacking Logic:** Supports max-stack limits, partial merges (overflow handling), and creating new stacks from leftovers.

* **Smart Drag & Drop:**

  * **Snap-to-Grid:** Visual snapping to the nearest valid slot.

  * **Fallback Logic:** Items automatically snap back to their original slot if a placement is invalid or the operation is canceled.

  * **Garbage Disposal:** Raycast-based deletion detection for dropping items into trash areas.

* **Stack Splitting:** Shift + Click logic to halve stacks dynamically during drag operations.

## ⚙️ Technical Highlights

### 1. Architecture

The system uses a **Singleton-based Manager** structure to centralize logic while keeping individual components lightweight.

* **InventoryManager:** Handles the "Business Logic" (Finding slots, validating moves, managing lists).

* **DragManager:** Handles the "Presentation Logic" (Coordinate conversion, visual parenting, sorting orders).

* **Slot & Tile:** Pure presentation components that report to managers.

### 2. Optimization

* **Spatial Lookups:** Slot detection uses optimized sqrMagnitude distance checks to avoid expensive square root calculations during drag operations.

* **No LINQ in Hot Paths:** While LINQ is used for initialization, runtime operations (like finding the closest slot) utilize non-allocating loops to minimize Garbage Collection (GC) pressure.

* **Event Subscriptions:** Careful management of event listeners (OnDestroy) to prevent memory leaks and "zombie" references.

### 3. Clean Code Practices

* **Guard Clauses:** extensively used to reduce nesting and improve readability.

* **Encapsulation:** All data modification is protected behind Properties with side-effects (e.g., updating UI text automatically when Quantity changes).

* **Error Handling:** Robust checks for "Null" states and "Invalid Placements" to prevent runtime crashes.
