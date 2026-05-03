CREATE TABLE "users"(
    "id" BIGINT NOT NULL,
    "username" VARCHAR(255) NOT NULL,
    "name" VARCHAR(255) NOT NULL,
    "password_hash" TEXT NOT NULL,
    "profile_pic" VARCHAR(255) NOT NULL,
    "created_at" TIMESTAMP(0) WITHOUT TIME ZONE NOT NULL,
    "updated_at" TIMESTAMP(0) WITHOUT TIME ZONE NULL
);
ALTER TABLE
    "users" ADD PRIMARY KEY("id");
ALTER TABLE
    "users" ADD CONSTRAINT "users_username_unique" UNIQUE("username");
CREATE TABLE "roles"(
    "id" BIGINT NOT NULL,
    "name" VARCHAR(255) NOT NULL
);
ALTER TABLE
    "roles" ADD PRIMARY KEY("id");
CREATE TABLE "user_roles"(
    "id" BIGINT NOT NULL,
    "user_id" BIGINT NOT NULL,
    "role_id" BIGINT NOT NULL
);
ALTER TABLE
    "user_roles" ADD PRIMARY KEY("id");
CREATE TABLE "products"(
    "id" BIGINT NOT NULL,
    "price" DECIMAL(8, 2) NOT NULL,
    "status" VARCHAR(255) CHECK
        ("status" IN('')) NOT NULL
);
ALTER TABLE
    "products" ADD PRIMARY KEY("id");
CREATE TABLE "categories"(
    "id" BIGINT NOT NULL,
    "name" VARCHAR(255) NOT NULL,
    "slug" VARCHAR(255) NOT NULL
);
ALTER TABLE
    "categories" ADD PRIMARY KEY("id");
ALTER TABLE
    "categories" ADD CONSTRAINT "categories_name_unique" UNIQUE("name");
ALTER TABLE
    "categories" ADD CONSTRAINT "categories_slug_unique" UNIQUE("slug");
CREATE TABLE "product_categories"(
    "id" BIGINT NOT NULL,
    "category_id" BIGINT NOT NULL,
    "product_id" BIGINT NOT NULL
);
CREATE INDEX "product_categories_product_id_category_id_index" ON
    "product_categories"("product_id", "category_id");
ALTER TABLE
    "product_categories" ADD PRIMARY KEY("id");
CREATE TABLE "images"(
    "id" BIGINT NOT NULL,
    "path" VARCHAR(255) NOT NULL,
    "product_id" BIGINT NOT NULL
);
ALTER TABLE
    "images" ADD PRIMARY KEY("id");
CREATE TABLE "posts"(
    "id" BIGINT NOT NULL,
    "title" VARCHAR(255) NOT NULL,
    "description" VARCHAR(255) NOT NULL,
    "created_at" TIMESTAMP(0) WITHOUT TIME ZONE NOT NULL,
    "updated_at" TIMESTAMP(0) WITHOUT TIME ZONE NULL,
    "product_id" BIGINT NOT NULL,
    "author_id" BIGINT NOT NULL
);
CREATE INDEX "posts_product_id_author_id_index" ON
    "posts"("product_id", "author_id");
ALTER TABLE
    "posts" ADD PRIMARY KEY("id");
CREATE TABLE "addresses"(
    "id" BIGINT NOT NULL,
    "city" VARCHAR(255) NULL,
    "state_province" VARCHAR(255) NOT NULL,
    "country" VARCHAR(255) NOT NULL,
    "user_id" BIGINT NOT NULL
);
ALTER TABLE
    "addresses" ADD PRIMARY KEY("id");
CREATE TABLE "contacts"(
    "id" BIGINT NOT NULL,
    "email" VARCHAR(255) NOT NULL,
    "phone_number" VARCHAR(255) NULL,
    "telegram_user" VARCHAR(255) NULL,
    "user_id" BIGINT NOT NULL
);
ALTER TABLE
    "contacts" ADD PRIMARY KEY("id");
ALTER TABLE
    "posts" ADD CONSTRAINT "posts_product_id_foreign" FOREIGN KEY("product_id") REFERENCES "products"("id");
ALTER TABLE
    "images" ADD CONSTRAINT "images_product_id_foreign" FOREIGN KEY("product_id") REFERENCES "products"("id");
ALTER TABLE
    "user_roles" ADD CONSTRAINT "user_roles_user_id_foreign" FOREIGN KEY("user_id") REFERENCES "users"("id");
ALTER TABLE
    "product_categories" ADD CONSTRAINT "product_categories_category_id_foreign" FOREIGN KEY("category_id") REFERENCES "categories"("id");
ALTER TABLE
    "user_roles" ADD CONSTRAINT "user_roles_role_id_foreign" FOREIGN KEY("role_id") REFERENCES "roles"("id");
ALTER TABLE
    "addresses" ADD CONSTRAINT "addresses_user_id_foreign" FOREIGN KEY("user_id") REFERENCES "users"("id");
ALTER TABLE
    "posts" ADD CONSTRAINT "posts_author_id_foreign" FOREIGN KEY("author_id") REFERENCES "users"("id");
ALTER TABLE
    "contacts" ADD CONSTRAINT "contacts_user_id_foreign" FOREIGN KEY("user_id") REFERENCES "users"("id");
ALTER TABLE
    "product_categories" ADD CONSTRAINT "product_categories_product_id_foreign" FOREIGN KEY("product_id") REFERENCES "products"("id");