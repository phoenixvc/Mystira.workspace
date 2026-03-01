//! Main application component

use leptos::prelude::*;
use leptos_meta::*;
use leptos_router::{
    components::{Route, Router, Routes},
    path,
};

use crate::components::layout::Layout;
use crate::pages::{
    dashboard::DashboardPage,
    infrastructure::InfrastructurePage,
    not_found::NotFoundPage,
    services::ServicesPage,
    settings::SettingsPage,
};

/// Main application component
#[component]
pub fn App() -> impl IntoView {
    // Provides context for meta tags
    provide_meta_context();

    view! {
        <Stylesheet href="/styles/main.css"/>
        <Title text="Mystira DevHub"/>
        <Meta name="description" content="Mystira Development Operations Hub"/>

        <Router>
            <Layout>
                <Routes fallback=|| view! { <NotFoundPage/> }>
                    <Route path=path!("/") view=DashboardPage/>
                    <Route path=path!("/infrastructure") view=InfrastructurePage/>
                    <Route path=path!("/services") view=ServicesPage/>
                    <Route path=path!("/settings") view=SettingsPage/>
                </Routes>
            </Layout>
        </Router>
    }
}
